using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class JuegoBatallaNaval
{
    static StreamWriter escritor;
    static char[,] miTablero;
    static char[,] tableroOponente;
    static int ultimoF, ultimoC;
    static AutoResetEvent miTurnoEvento = new AutoResetEvent(false);
    static AutoResetEvent juegoIniciadoEvento = new AutoResetEvent(false);
    static bool juegoTerminado = false;
    static bool esJugador1;
    static Random aleatorio = new Random();

    static void Main()
    {
        miTablero = ElegirTableroAleatorio();
        tableroOponente = CrearTableroOculto();

        TcpClient cliente;
        try
        {
            cliente = new TcpClient("127.0.0.1", 5007);
        }
        catch
        {
            Console.WriteLine("No se pudo conectar al servidor. Asegurate de que el servidor este corriendo.");
            Console.ReadKey();
            return;
        }

        escritor = new StreamWriter(cliente.GetStream(), new UTF8Encoding(false)) { AutoFlush = true };
        StreamReader lector = new StreamReader(cliente.GetStream(), Encoding.UTF8);

        Thread hiloRecepcion = new Thread(() => RecibirMensajes(lector));
        hiloRecepcion.IsBackground = true;
        hiloRecepcion.Start();

        Console.WriteLine("Conectado al servidor. Esperando segundo jugador...");
        juegoIniciadoEvento.WaitOne();

        if (juegoTerminado) return;

        Console.Clear();
        if (esJugador1)
            Console.WriteLine("=== WARSHIP - Jugador 1 === (atacas primero)");
        else
            Console.WriteLine("=== WARSHIP - Jugador 2 === (espera tu turno)");

        Console.WriteLine("\nTu tablero con tus barcos (#):");
        ImprimirTablero(miTablero);

        while (!juegoTerminado)
        {
            miTurnoEvento.WaitOne();

            if (juegoTerminado) break;

            Console.WriteLine("\n=== TU TURNO ===");
            Console.WriteLine("\nTablero rival (tus disparos):");
            ImprimirTablero(tableroOponente);

            RealizarDisparo();
        }

        Console.WriteLine("\nPresiona una tecla para salir.");
        Console.ReadKey();
    }

    static void RealizarDisparo()
    {
        while (true)
        {
            try
            {
                Console.Write("\nFila (0-9): ");
                int f = int.Parse(Console.ReadLine());
                Console.Write("Columna (0-9): ");
                int c = int.Parse(Console.ReadLine());

                if (f < 0 || f > 9 || c < 0 || c > 9)
                {
                    Console.WriteLine("Coordenadas fuera del tablero (0-9).");
                    continue;
                }

                if (tableroOponente[f, c] == 'X' || tableroOponente[f, c] == 'O')
                {
                    Console.WriteLine("Ya disparaste ahi, elige otra celda.");
                    continue;
                }

                ultimoF = f;
                ultimoC = c;
                escritor.WriteLine("SHOT|" + f + "|" + c);
                break;
            }
            catch
            {
                Console.WriteLine("Entrada invalida, ingresa un numero.");
            }
        }
    }

    static void RecibirMensajes(StreamReader lector)
    {
        try
        {
            while (!juegoTerminado)
            {
                string msg = lector.ReadLine();
                if (msg == null) break;
                msg = msg.Trim();
                if (msg.Length == 0) continue;
                ProcesarMensaje(msg);
            }
        }
        catch
        {
            Console.WriteLine("\nConexion con el servidor perdida.");
            juegoTerminado = true;
            miTurnoEvento.Set();
            juegoIniciadoEvento.Set();
        }
    }

    static void ProcesarMensaje(string msg)
    {
        string[] parts = msg.Split('|');

        switch (parts[0])
        {
            case "START":
                esJugador1 = parts[1] == "0";
                juegoIniciadoEvento.Set();
                break;

            case "YOUR_TURN":
                miTurnoEvento.Set();
                break;

            case "WAIT":
                Console.WriteLine("\nEsperando al oponente...");
                break;

            case "HIT":
                tableroOponente[ultimoF, ultimoC] = 'X';
                Console.WriteLine("\n¡IMPACTO! Puedes disparar de nuevo.");
                Console.WriteLine("\nTablero rival:");
                ImprimirTablero(tableroOponente);
                break;

            case "MISS":
                tableroOponente[ultimoF, ultimoC] = 'O';
                Console.WriteLine("\nAgua. Turno del oponente.");
                break;

            case "WIN":
                tableroOponente[ultimoF, ultimoC] = 'X';
                Console.WriteLine("\n¡¡GANASTE!! Hundiste todos los barcos enemigos.");
                Console.WriteLine("\nTablero final rival:");
                ImprimirTablero(tableroOponente);
                juegoTerminado = true;
                miTurnoEvento.Set();
                break;

            case "LOSE":
                Console.WriteLine("\nPerdiste. El oponente hundio todos tus barcos.");
                Console.WriteLine("\nTu tablero final:");
                ImprimirTablero(miTablero);
                juegoTerminado = true;
                miTurnoEvento.Set();
                break;

            case "INCOMING":
                if (parts.Length < 3) break;
                int f = int.Parse(parts[1]);
                int c = int.Parse(parts[2]);
                Console.WriteLine("\n--- El oponente dispara en (" + f + ", " + c + ")...");

                if (miTablero[f, c] == '#')
                {
                    miTablero[f, c] = 'X';
                    Console.WriteLine("¡Te dio!");
                    ImprimirTablero(miTablero);

                    if (!QuedanBarcos(miTablero))
                        escritor.WriteLine("RESULT|WIN");
                    else
                        escritor.WriteLine("RESULT|HIT");
                }
                else
                {
                    if (miTablero[f, c] == '~') miTablero[f, c] = 'O';
                    Console.WriteLine("Fallo. ¡Prepara tu ataque!");
                    ImprimirTablero(miTablero);
                    escritor.WriteLine("RESULT|MISS");
                }
                break;
        }
    }

    static bool QuedanBarcos(char[,] tablero)
    {
        for (int i = 0; i < 10; i++)
            for (int j = 0; j < 10; j++)
                if (tablero[i, j] == '#')
                    return true;
        return false;
    }

    static char[,] InicializarTablero()
    {
        char[,] t = new char[10, 10];
        for (int i = 0; i < 10; i++)
            for (int j = 0; j < 10; j++)
                t[i, j] = '~';
        return t;
    }

    static char[,] CrearTableroOculto() { return InicializarTablero(); }

    static char[,] ElegirTableroAleatorio()
    {
        int opcion = aleatorio.Next(0, 6);
        switch (opcion)
        {
            case 0: return Tablero1();
            case 1: return Tablero2();
            case 2: return Tablero3();
            case 3: return Tablero4();
            case 4: return Tablero5();
            default: return Tablero6();
        }
    }

    static void ImprimirTablero(char[,] tablero)
    {
        Console.WriteLine("\n   0 1 2 3 4 5 6 7 8 9");
        for (int i = 0; i < 10; i++)
        {
            Console.Write(i + "  ");
            for (int j = 0; j < 10; j++)
                Console.Write(tablero[i, j] + " ");
            Console.WriteLine();
        }
    }

    static char[,] Tablero1()
    {
        char[,] t = InicializarTablero();
        t[0, 0] = '#'; t[0, 1] = '#';
        t[2, 3] = '#'; t[3, 3] = '#';
        t[5, 5] = '#'; t[5, 6] = '#'; t[5, 7] = '#';
        t[7, 1] = '#'; t[8, 1] = '#'; t[9, 1] = '#';
        t[9, 6] = '#'; t[9, 7] = '#'; t[9, 8] = '#'; t[9, 9] = '#';
        return t;
    }

    static char[,] Tablero2()
    {
        char[,] t = InicializarTablero();
        t[0, 0] = '#'; t[0, 1] = '#';
        t[2, 2] = '#'; t[3, 2] = '#';
        t[5, 5] = '#'; t[5, 6] = '#'; t[5, 7] = '#';
        t[7, 1] = '#'; t[8, 1] = '#'; t[9, 1] = '#';
        t[9, 6] = '#'; t[9, 7] = '#'; t[9, 8] = '#'; t[9, 9] = '#';
        return t;
    }

    static char[,] Tablero3()
    {
        char[,] t = InicializarTablero();
        t[1, 1] = '#'; t[1, 2] = '#';
        t[3, 5] = '#'; t[4, 5] = '#';
        t[6, 0] = '#'; t[6, 1] = '#'; t[6, 2] = '#';
        t[0, 9] = '#'; t[1, 9] = '#'; t[2, 9] = '#';
        t[8, 4] = '#'; t[8, 5] = '#'; t[8, 6] = '#'; t[8, 7] = '#';
        return t;
    }

    static char[,] Tablero4()
    {
        char[,] t = InicializarTablero();
        t[4, 4] = '#'; t[4, 5] = '#';
        t[0, 0] = '#'; t[1, 0] = '#';
        t[2, 7] = '#'; t[3, 7] = '#'; t[4, 7] = '#';
        t[6, 3] = '#'; t[6, 4] = '#'; t[6, 5] = '#';
        t[9, 0] = '#'; t[9, 1] = '#'; t[9, 2] = '#'; t[9, 3] = '#';
        return t;
    }

    static char[,] Tablero5()
    {
        char[,] t = InicializarTablero();
        t[9, 9] = '#'; t[8, 9] = '#';
        t[0, 3] = '#'; t[0, 4] = '#';
        t[5, 1] = '#'; t[5, 2] = '#'; t[5, 3] = '#';
        t[2, 6] = '#'; t[3, 6] = '#'; t[4, 6] = '#';
        t[7, 7] = '#'; t[7, 8] = '#'; t[7, 9] = '#'; t[7, 6] = '#';
        return t;
    }

    static char[,] Tablero6()
    {
        char[,] t = InicializarTablero();
        t[0, 5] = '#'; t[1, 5] = '#';
        t[3, 1] = '#'; t[4, 1] = '#';
        t[6, 6] = '#'; t[6, 7] = '#'; t[6, 8] = '#';
        t[1, 9] = '#'; t[2, 9] = '#'; t[3, 9] = '#';
        t[8, 0] = '#'; t[8, 1] = '#'; t[8, 2] = '#'; t[8, 3] = '#';
        return t;
    }
}
