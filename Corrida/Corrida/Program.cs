using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corrida
{
    using System;
    using System.Threading;

    /* Grupo 404 - SMAUG:
      Caike Grion dos Santos
      João Pedro Queiroz de Melo
      João Vitor dos Reis Domingues
      Lucas Neves Timar
      Lucas Proetti Quadros
      Matheus Santos Duca
    */

    #region Classe Main
    // Classe principal que contém o método Main
    public class Principal
    {

        // Método de entrada do programa
        private static void Main(string[] args)
        {
            // Inicie o jogo
            Jogo.Rodar();
        }
    }
    #endregion Fim Classe Principal

    #region Classe Jogo

    public class Jogo
    {
        private static readonly object lockObj = new object(); // Objeto de sincronização
        private static bool[][] resultados; // Matriz para armazenar os resultados da detecção de colisão

        private static Carro[] carros; // Lista de carros do jogo

        private static bool jogando = true;

        private static int tempoMaximo = 0;

        // Ordem: Conversíveis, Off-Road, Fórmula 1
        private static int[] larguras = { 6, 10, 4 };
        private static int[] alturas = { 8, 12, 6 };
        private static int[] velocidades = { 10, 5, 20 };
        private static int[] derrapagens = { 50, 75, 25 };
        
        private static string[] titulos = {"Parallel Conversíveis Edition", "Parallel Off-Road Edition", "Parallel F1 Edition" };
        private static string tituloCorrida;

        // Largura da Pista
        private static int larguraPadrao = 60; // MMC entre as larguras
        public static int larguraPistaAtual; // larguraPadrao * largura dos carros da corrida atual

        // Método principal do jogo
        public static void Rodar()
        {
            // Configurações iniciais
            ConfigurarCorrida();

            // Game Loop
            do
            {
                Console.Clear();
                Console.WriteLine(tituloCorrida);
                Console.WriteLine("--------------------------");
                Console.WriteLine("Placar:");

                for (int i = 0; i < carros.Length; i++)
                {
                    Console.WriteLine("Piloto " + (i + 1) + " Y: " + carros[i].Y + " X: " + carros[i].X);

                    carros[i].Correr();
                }

                // Inicialize a matriz de resultados da detecção de colisão
                resultados = new bool[carros.Length][];

                for (int i = 0; i < carros.Length; i++)
                {
                    resultados[i] = new bool[carros.Length];
                }

                // Crie um array para armazenar as threads
                Thread[] threads = new Thread[carros.Length];

                // Inicie as threads para verificar colisão entre os carros
                for (int i = 0; i < carros.Length; i++)
                {
                    threads[i] = new Thread(VerificarColisaoThread);
                    threads[i].Start(i);
                }

                // Aguarde a conclusão de todas as threads
                for (int i = 0; i < carros.Length; i++)
                {
                    threads[i].Join();
                }

                // Movimentação dos carros vai aqui

                // Processe os resultados da detecção de colisão e tome ação apropriada, como atualizar o placar ou gerenciar as colisões no jogo

                // Atualizar o placar vai aqui

                // ...
                Thread.Sleep(1000);
            } while (jogando);
        }

        // Método para ser executado em cada thread
        private static void VerificarColisaoThread(object indice)
        {
            int idx = (int)indice;
            for (int i = idx + 1; i < carros.Length; i++)
            {
                if (carros[idx].VerificarColisao(carros[i]))
                {
                    lock (lockObj)
                    {
                        resultados[idx][i] = true;
                    }
                }
            }
        }

        private static void ConfigurarCorrida()
        {
            Console.Clear();

            Console.WriteLine("---- Parallel Race ----");
            Console.WriteLine("Escolha o tipo de corrida");
            Console.WriteLine("[1] Conversíveis (Carros médios, Velocidade média)");
            Console.WriteLine("[2] Off-Road (Carros Grandes, Velocidade baixa");
            Console.WriteLine("[3] Fórmula 1 (Carros pequenos, Velocidade alta)");

            int opCorrida = int.Parse(Console.ReadLine());
            while (opCorrida < 1 || opCorrida > 3)
            {
                Console.WriteLine("Selecione uma opção de corrida válida!");
                opCorrida = int.Parse(Console.ReadLine());
            }
            opCorrida -= 1;
            tituloCorrida = titulos[opCorrida];
            larguraPistaAtual = larguraPadrao * larguras[opCorrida];

            Console.WriteLine("Escolha o número de pilotos:");
            Console.WriteLine("[1] - 4 pilotos");
            Console.WriteLine("[2] - 8 pilotos");
            Console.WriteLine("[3] - 12 pilotos");
            Console.WriteLine("[4] - 16 pilotos");

            int opQuant = int.Parse(Console.ReadLine());
            while (opQuant < 1 || opQuant > 4)
            {
                Console.WriteLine("Escolha uma opção de número de pilotos válida!");
                opQuant = int.Parse(Console.ReadLine());
            }

            carros = new Carro[4 * opQuant];

            for (int i = 0; i < carros.Length; i++)
            {
                carros[i] = new Carro(0  + i * larguras[opCorrida], 0, larguras[opCorrida], alturas[opCorrida], 
                    velocidades[opCorrida], derrapagens[opCorrida]);
            }

            /*
            Console.WriteLine("Escolha o tempo máximo da corrida:");
            Console.WriteLine("[1] - 1 minuto");
            Console.WriteLine("[2] - 2 minutos");
            Console.WriteLine("[3] - 3 minutos");
            Console.WriteLine("[4] - 4 minutos");

            int opTemp = int.Parse(Console.ReadLine());
            while (opTemp < 1 || opTemp > 4)
            {
                Console.WriteLine("Escolha uma opção de tempo válida!");
                opTemp = int.Parse(Console.ReadLine());
            }

            tempoMaximo = 60 * opTemp;
            */
        }
    }
    #endregion Fim Classe Jogo

    #region Classe Carro
    // Classe que usaremos para instanciar os objetos Carro
    public class Carro
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Largura { get; set; }
        public int Altura { get; set; }
        public int Velocidade { get; set; }
        public int Derrapagem { get; set; }

        private static Random rand = new Random();

        // Método Construtor para configurar os atributos do Carro
        public Carro(int x, int y, int largura, int altura, int velocidade, int derrapagem)
        {
            this.X = x;
            this.Y = y;
            this.Largura = largura;
            this.Altura = altura;
            this.Velocidade = velocidade;
            this.Derrapagem = derrapagem;
        }

        // Método que verifica se a instância do Carro está colidindo com outra
        public bool VerificarColisao(Carro outroCarro)
        {
            // Alteramos o ponto de origem do Carro para o meio, aonde a hitbox é feita a partir desse centro
            if (X < outroCarro.X + outroCarro.Largura / 2 &&
              X + Largura / 2 > outroCarro.X - outroCarro.Largura / 2 &&
              Y < outroCarro.Y + outroCarro.Altura / 2 &&
              Y + Altura / 2 > outroCarro.Y - outroCarro.Altura / 2)
            {
                return true;
            }
            return false;
        }

        // Método que irá movimentar o Carro nos eixos Y (indo para frente) e X (indo para os lados)
        public void Correr()
        {
            // Ir para frente
            Y += Velocidade;

            // Derrapagem
            if (rand.Next(100) < Derrapagem)
            {
                int dirDerrapagem = 1;

                if (rand.Next(100) < 50)
                    dirDerrapagem = -1;

                X += Velocidade * dirDerrapagem;
                if (X <= 0)
                    X = Largura / 2;
                else if (X >= Jogo.larguraPistaAtual)
                {
                    X = Jogo.larguraPistaAtual - Largura / 2;
                }
            }
        }

        public void AndarParaFrente()
        {
            Y += Velocidade;
        }

        public void Derrapar()
        {

        }
    }
    #endregion Fim Classe Carro
}
