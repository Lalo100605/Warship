using System;

class JuegoBatallaNaval
{
    static Random aleatorio = new Random();

    static void Main()
    {
        char[,] tableroJugador = TableroJugadorFijo();

        // Tableros CPU
        char[][,] tablerosCpu = new char[5][,];
        tablerosCpu[0] = TableroCpu1();
        tablerosCpu[1] = TableroCpu2();
        tablerosCpu[2] = TableroCpu3();
        tablerosCpu[3] = TableroCpu4();
        tablerosCpu[4] = TableroCpu5();

        int elegido = aleatorio.Next(0, 5);
        char[,] tableroCpuReal = tablerosCpu[elegido];

        // Tableros para disparos
        char[,] tableroVisibleCpu = CrearTableroOculto();
        char[,] tableroVisibleJugador = CrearTableroOculto();

        Console.WriteLine("Tu tablero (real):");
        ImprimirTablero(tableroJugador);
        Console.WriteLine("\nPresiona una tecla para comenzar la batalla...");
        Console.ReadKey();
        Console.Clear();

        bool turnoJugador = true;

        while (true)
        {
            if (turnoJugador)
            {
                Console.WriteLine("=== TU TURNO ===");
                ImprimirTablero(tableroVisibleCpu);

                bool repite = TiroJugador(tableroCpuReal, tableroVisibleCpu);
                if (!QuedanBarcos(tableroCpuReal))
                {
                    Console.WriteLine("\n¡Ganaste! Hundiste todos los barcos de la CPU.");
                    break;
                }

                if (!repite)
                {
                    turnoJugador = false;
                    Console.WriteLine("\nFallaste. Turno de la CPU...");
                    Console.ReadKey();
                    Console.Clear();
                }
            }
            else
            {
                Console.WriteLine("=== TURNO DE LA CPU ===");
                ImprimirTablero(tableroVisibleJugador);

                bool repite = TiroCpu(tableroJugador, tableroVisibleJugador);
                if (!QuedanBarcos(tableroJugador))
                {
                    Console.WriteLine("\nLa CPU ganó. Hundió todos tus barcos.");
                    Console.WriteLine("\nTu tablero real:");
                    ImprimirTablero(tableroJugador);
                    break;
                }

                if (!repite)
                {
                    turnoJugador = true;
                    Console.WriteLine("\nLa CPU falló. ¡Tu turno!");
                    Console.ReadKey();
                    Console.Clear();
                }
            }
        }

        Console.WriteLine("\nJuego terminado. Presiona una tecla para salir.");
        Console.ReadKey();
    }

    static bool TiroJugador(char[,] tableroCpuReal, char[,] tableroVisibleCpu)
    {
        while (true)
        {
            try
            {
                Console.Write("Fila (0-9): ");
                int f = int.Parse(Console.ReadLine());
                Console.Write("Columna (0-9): ");
                int c = int.Parse(Console.ReadLine());

                if (f < 0 || f > 9 || c < 0 || c > 9)
                {
                    Console.WriteLine("X Coordenadas fuera del tablero.");
                    continue;
                }

                if (tableroVisibleCpu[f, c] == 'X' || tableroVisibleCpu[f, c] == 'O')
                {
                    Console.WriteLine("X Ya disparaste ahí.");
                    continue;
                }

                if (tableroCpuReal[f, c] == '#')
                {
                    Console.WriteLine("¡Impacto!");
                    tableroVisibleCpu[f, c] = 'X';
                    tableroCpuReal[f, c] = 'X';
                    ImprimirTablero(tableroVisibleCpu);
                    return true;
                }
                else
                {
                    Console.WriteLine("Agua.");
                    tableroVisibleCpu[f, c] = 'O';
                    ImprimirTablero(tableroVisibleCpu);
                    return false;
                }
            }
            catch
            {
                Console.WriteLine("Entrada inválida.");
            }
        }
    }

    static bool TiroCpu(char[,] tableroJugadorReal, char[,] tableroVisibleJugador)
    {
        int f, c;
        do
        {
            f = aleatorio.Next(0, 10);
            c = aleatorio.Next(0, 10);
        } while (tableroVisibleJugador[f, c] == 'X' || tableroVisibleJugador[f, c] == 'O');

        Console.WriteLine($"La CPU dispara a ({f}, {c})...");
        Console.ReadKey();

        if (tableroJugadorReal[f, c] == '#')
        {
            Console.WriteLine("¡Te dio!");
            tableroJugadorReal[f, c] = 'X';
            tableroVisibleJugador[f, c] = 'X';
            ImprimirTablero(tableroVisibleJugador);
            return true;
        }
        else
        {
            Console.WriteLine("Falló.");
            tableroVisibleJugador[f, c] = 'O';
            ImprimirTablero(tableroVisibleJugador);
            return false;
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

    static char[,] CrearTableroOculto()
    {
        return InicializarTablero();
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

    static char[,] TableroJugadorFijo()
    {
        char[,] t = InicializarTablero();
        t[0, 0] = '#'; t[0, 1] = '#';
        t[2, 3] = '#'; t[3, 3] = '#';
        t[5, 5] = '#'; t[5, 6] = '#'; t[5, 7] = '#';
        t[7, 1] = '#'; t[8, 1] = '#'; t[9, 1] = '#';
        t[9, 6] = '#'; t[9, 7] = '#'; t[9, 8] = '#'; t[9, 9] = '#';
        return t;
    }

    static char[,] TableroCpu1()
    {
        char[,] t = InicializarTablero();
        t[0, 0] = '#'; t[0, 1] = '#';
        t[2, 2] = '#'; t[3, 2] = '#';
        t[5, 5] = '#'; t[5, 6] = '#'; t[5, 7] = '#';
        t[7, 1] = '#'; t[8, 1] = '#'; t[9, 1] = '#';
        t[9, 6] = '#'; t[9, 7] = '#'; t[9, 8] = '#'; t[9, 9] = '#';
        return t;
    }

    static char[,] TableroCpu2()
    {
        char[,] t = InicializarTablero();
        t[1, 1] = '#'; t[1, 2] = '#';
        t[3, 5] = '#'; t[4, 5] = '#';
        t[6, 0] = '#'; t[6, 1] = '#'; t[6, 2] = '#';
        t[0, 9] = '#'; t[1, 9] = '#'; t[2, 9] = '#';
        t[8, 4] = '#'; t[8, 5] = '#'; t[8, 6] = '#'; t[8, 7] = '#';
        return t;
    }

    static char[,] TableroCpu3()
    {
        char[,] t = InicializarTablero();
        t[4, 4] = '#'; t[4, 5] = '#';
        t[0, 0] = '#'; t[1, 0] = '#';
        t[2, 7] = '#'; t[3, 7] = '#'; t[4, 7] = '#';
        t[6, 3] = '#'; t[6, 4] = '#'; t[6, 5] = '#';
        t[9, 0] = '#'; t[9, 1] = '#'; t[9, 2] = '#'; t[9, 3] = '#';
        return t;
    }

    static char[,] TableroCpu4()
    {
        char[,] t = InicializarTablero();
        t[9, 9] = '#'; t[8, 9] = '#';
        t[0, 3] = '#'; t[0, 4] = '#';
        t[5, 1] = '#'; t[5, 2] = '#'; t[5, 3] = '#';
        t[2, 6] = '#'; t[3, 6] = '#'; t[4, 6] = '#';
        t[7, 7] = '#'; t[7, 8] = '#'; t[7, 9] = '#'; t[7, 6] = '#';
        return t;
    }

    static char[,] TableroCpu5()
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