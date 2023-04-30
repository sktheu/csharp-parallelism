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
public class Program
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
    private static bool[] resultados; // Array para armazenar os resultados da detecção de colisão
    private static char[] batidos; // Array para armazenar quem sofreu a batida

    private static Carro[] carros; // Matriz que irá armazenar os carros do jogo

    private static bool jogando = true; // condição para continuar rodando o gameloop

    // Array feito com o o intuito de especificar cada piloto na corrida, atribuindo um dos caracteres abaixo
    private static char[] idPilotos = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X' };

    // Ordem: Conversíveis, Off-Road, Fórmula 1
    private static int[] larguras = { 6, 10, 4 };
    private static int[] alturas = { 8, 12, 6 };
    private static int[] velocidades = { 10, 5, 20 };
    private static int[] derrapagens = { 50, 75, 25 };

    private static string[] titulos = { "Parallel Race - Conversíveis Edition", " Parallel Race - Off-Road Edition   ", "    Parallel Race - F1 Edition      " };
    private static string tituloCorridaAtual;

    // Largura da Pista
    private static int larguraPadrao = 60; // MMC entre as larguras
    public static int larguraPistaAtual; // larguraPadrao * largura dos carros da corrida atual

    // Tempo
    private static int tempoMaximo;
    private static int tempoAtual;

    // Método principal do jogo
    public static void Rodar()
    {
        // Configurações iniciais
        ConfigurarCorrida();

        // Game Loop
        do
        {
            Console.Clear();
            Console.WriteLine("---------------------------------------");
            Console.WriteLine("| " + tituloCorridaAtual + "|");
            Console.WriteLine("---------------------------------------");

            if (tempoAtual < tempoMaximo)
            {
                Console.WriteLine(".Tempo: " + tempoAtual + "\n");
            }
            else
            {
                Console.WriteLine(".Tempo esgotado - FIM DA CORRIDA");
                Console.WriteLine(" Deseja correr novamente? (S/N)" + "\n");
                AtualizarPlacarThread();
                string input = Console.ReadLine().ToUpper();
                while (input != "S" && input != "N")
                {
                    Console.WriteLine("Opção Inválida!");

                    input = Console.ReadLine().ToUpper();
                }

                if (input == "N") // Encerre o Game Loop e quebre-o
                {
                    jogando = false;
                    break;
                }

                ConfigurarCorrida(); // Vá novamente para o menu
            }

            // Thread que mostra o progresso da corrida
            Thread placar = new Thread(AtualizarPlacarThread);
            placar.Start();

            // Inicialize o array de resultados da detecção de colisão
            resultados = new bool[carros.Length];

            // Inicialize o array de Ids de quem sofreu a batida
            batidos = new char[carros.Length];

            // Crie um array para armazenar as threads de colisão
            Thread[] threadsColisao = new Thread[3 * (carros.Length / 6)];

            /* Lógica da otimização: Focar a colisão entre os carros mais próximos, ou seja
             aqueles que tem valores similares no eixo Y (a partir da reorganização do
            placar conseguimos fazer isso) */

            // 6 carros == 3 Threads (2 carros em cada Thread, verificando se estão colidindo entre si)
            // 12 carros == 6 Threads (2 carros em cada Thread, verificando se estão colidindo entre si)
            // 18 carros == 9 Threads (2 carros em cada Thread, verificando se estão colidindo entre si)
            // 24 carros == 12 Threads (2 carros em cada Thread, verificando se estão colidindo entre si)

            // Indice usado na alocação dos carros nas Threads
            int idxCarro = 0;

            // Inicie as threads para verificar colisão entre os carros
            for (int i = 0; i < threadsColisao.Length; i++)
            {
                threadsColisao[i] = new Thread(() => VerificarColisaoThread(idxCarro, idxCarro + 1));
                threadsColisao[i].Start();

                idxCarro = i + threadsColisao.Length - 1; // +2 (6 carros), +5 (12 carros), +8 (18 carros), +11 (24 carros)
            }

            // Crie um array para armazenar as threads de movimentação
            Thread[] threadsMover = new Thread[3 * (carros.Length / 6)];

            // 6 carros == 3 Threads (2 carros se movendo em cada Thread)
            // 12 carros == 6 Threads (2 carros se movendo em cada Thread)
            // 18 carros == 9 Threads (2 carros se movendo em cada Thread)
            // 24 carros == 12 Threads (2 carros se movendo em cada Thread)

            idxCarro = 0; // Reinicando o valor do idxCarro para manipular o array threadsMover

            // Inicie as threads para movimentar os carros
            for (int i = 0; i < threadsMover.Length; i++)
            {
                threadsMover[i] = new Thread(() => MoverCarroThread(carros[idxCarro], carros[idxCarro + 1]));
                threadsMover[i].Start();
                idxCarro = i + threadsMover.Length - 1; // +2 (6 carros), +5 (12 carros), +8 (18 carros), +11 (24 carros)
            }

            // Aguarde a conclusão de todas as threads de colisão
            for (int i = 0; i < threadsColisao.Length; i++)
            {
                threadsColisao[i].Join();
            }

            // Aguarde a conclusão de todas as threads de movimentação
            for (int i = 0; i < threadsMover.Length; i++)
            {
                threadsMover[i].Join();
            }

            Console.WriteLine("---------------------------------------");
            // Array que armazena as threads que lidam com os resultados das colisões
            Thread[] threadsLidar = new Thread[2 * (carros.Length / 6)];
            // 6 resultados == 2 Threads
            // 12 resultados == 4 Threads
            // 18 resultados == 6 Threads
            // 24 resultados == 8 Threads

            // Inicie as threads para tratamento das colisões
            for (int i = 0; i < threadsLidar.Length; i++)
            {
                threadsLidar[i] = new Thread(() => LidarColisoesThread(i * 2, i * 2 + 2));
                threadsLidar[i].Start();
            }

            // Aguarde a conclusão de todas as threads de tratamento de colisão
            for (int i = 0; i < threadsLidar.Length; i++)
            {
                threadsLidar[i].Join();
            }

            // Acrescente o tempo atual
            tempoAtual++;

            Thread.Sleep(1000);
        } while (jogando); // Condição do Game Loop
    }

    // Método para ser executado em cada thread
    private static void VerificarColisaoThread(int indiceC1, int indiceC2)
    {
        if (carros[indiceC2].VerificarColisao(carros[indiceC1]))
        {
            lock (lockObj) // Semáforo
            {
                resultados[indiceC2] = true; // Colidiu
                batidos[indiceC2] = carros[indiceC1].Id; // Quem sofreu a batida
            }
        }

        if (carros[indiceC1].VerificarColisao(carros[indiceC2]))
        {
            lock (lockObj) // Semáforo
            {
                resultados[indiceC1] = true; // Colidiu
                batidos[indiceC1] = carros[indiceC2].Id; // Quem sofreu a batida
            }
        }
    }

    // Método para o usuário customizar a corrida atual
    private static void ConfigurarCorrida()
    {
        Console.Clear();
        tempoAtual = 0; // Resetando tempoAtual

        Console.WriteLine("---------------------------------------");
        Console.WriteLine("|       Parallel Race - MENU          |");
        Console.WriteLine("---------------------------------------");
        Console.WriteLine("Escolha o tipo de corrida:");
        Console.WriteLine("[1] Conversíveis (Carros médios, Velocidade média)");
        Console.WriteLine("[2] Off-Road (Carros Grandes, Velocidade baixa)");
        Console.WriteLine("[3] Fórmula 1 (Carros pequenos, Velocidade alta)");

        int opCorrida = int.Parse(Console.ReadLine());
        while (opCorrida < 1 || opCorrida > 3)
        {
            Console.WriteLine("Selecione uma opção de corrida válida!");
            opCorrida = int.Parse(Console.ReadLine());
        }
        opCorrida -= 1;
        tituloCorridaAtual = titulos[opCorrida]; // Atribua o título da corrida
        larguraPistaAtual = larguraPadrao * larguras[opCorrida]; // Defina a largura da pista com base no tipo de corrida escolhido

        Console.WriteLine("Escolha o número de pilotos:");
        Console.WriteLine("[1] - 6 pilotos");
        Console.WriteLine("[2] - 12 pilotos");
        Console.WriteLine("[3] - 18 pilotos");
        Console.WriteLine("[4] - 24 pilotos");

        int opQuant = int.Parse(Console.ReadLine());
        while (opQuant < 1 || opQuant > 4)
        {
            Console.WriteLine("Escolha uma opção de número de pilotos válida!");
            opQuant = int.Parse(Console.ReadLine());
        }

        carros = new Carro[6 * opQuant]; // Inicialize o array de carros com base na quantidade escolhida

        // Crie as instância de carro com base no tipo de corrida escolhido (Tamanho, velocidade e derrapagem são relativos a opção)
        for (int i = 0; i < carros.Length; i++)
        {
            carros[i] = new Carro(idPilotos[i], 0 + i * larguras[opCorrida], 0, larguras[opCorrida], alturas[opCorrida],
                velocidades[opCorrida], derrapagens[opCorrida]);
        }

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

        tempoMaximo = 60 * opTemp; // Configure o tempo máximo ==> 60sec * quantidade desejada (1 a 4 min)
    }

    // Método que organiza o array de carros com base em quem está na frente (maior valor no eixo Y)
    private static void AtualizarPlacarThread()
    {
        Console.WriteLine(".Placar:");

        // Organize o array, com base nos valores no eixo Y dos carros (maior para o menor)
        Array.Sort(carros, (c1, c2) => c2.Y.CompareTo(c1.Y));

        // Mostre na tela o placar
        for (int i = 0; i < carros.Length; i++)
        {
            if (i < 9) // Caso estiver abaixo do TOP 10 aumente o espaçamento entre as strings
                Console.WriteLine("  " + (i + 1) + "º - Piloto " + carros[i].Id + "  |   Y: " + carros[i].Y + " , " + "X: " + carros[i].X);
            else // Caso estiver acima do TOP 10, reduza o espaçamento entre as strings
                Console.WriteLine(" " + (i + 1) + "º - Piloto " + carros[i].Id + "  |   Y: " + carros[i].Y + " , " + "X: " + carros[i].X);
        }
    }

    // Método usado para movimentar dois carros em cada execução de Thread
    private static void MoverCarroThread(Carro c1, Carro c2)
    {
        c1.Correr();
        c2.Correr();
    }

    // Método para processar os resultados recebidos pela deteccção de colisão
    private static void LidarColisoesThread(int inicio, int final)
    {
        for (int i = inicio; i < final; i++)
        {
            if (resultados[i] && !carros[i].JaBateu) // Caso tiver colidido && ainda não tiver lidado com a colisão
            {
                carros[i].JaBateu = true; // Registrando que já bateu nesse frame
                Console.WriteLine("Piloto " + carros[i].Id + " bateu no Piloto " + batidos[i] + "! Realocando..."); // Mostre quem bateu
                carros[i].Y -= carros[i].Altura; // Aplique uma punição no eixo Y
            }
        }
    }
}
#endregion Fim Classe Jogo

#region Classe Carro
// Classe que usaremos para instanciar os objetos Carro
public class Carro
{
    // Atributos da Classe carro
    public char Id { get; private set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Largura { get; private set; }
    public int Altura { get; private set; }
    public int Velocidade { get; private set; }
    public int Derrapagem { get; private set; }

    public bool JaBateu = false;

    private static Random rand = new Random(); // objeto Random compartilhado entre todas as instâncias da classe Carro

    // Método Construtor para configurar os atributos do Carro
    public Carro(char id, int x, int y, int largura, int altura, int velocidade, int derrapagem)
    {
        this.Id = id;
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
            return true; // Colidiu
        }
        return false; // Não colidiu
    }

    // Método que irá movimentar o Carro nos eixos Y (indo para frente) e X (indo para os lados)
    public void Correr()
    {
        JaBateu = false; // Resetando para lidar com os resultados do frame atual
        AndarParaFrente();
        Derrapar();
    }

    private void AndarParaFrente()
    {
        if (rand.Next(100) < 25) // Chance de 25% de se movimentar lentamente nesse frame
            Y += Velocidade / 2;
        else
            Y += Velocidade; // Velocidade Normal
    }

    private void Derrapar()
    {
        if (rand.Next(100) < Derrapagem) // Chance de Derrapar no frame atual
        {
            int dirDerrapagem = 1; // Direita é a direção padrão da derrapagem

            if (rand.Next(100) < 50) // 50%  de chance de ir para a esquerda
                dirDerrapagem = -1;

            X += Velocidade * dirDerrapagem; // Aplicar a derrapagem

            if (X <= 0) // Caso ultrapassar o limite lateral esquerdo da pista
                X = Largura / 2; // Realoque para a posição mínima no eixo X
            else if (X >= Jogo.larguraPistaAtual) // Caso ultrapassar o limite lateral direito da pista
                X = Jogo.larguraPistaAtual - Largura / 2; // Realoque para a posição máxima no eixo X
        }
    }
}
#endregion Fim Classe Carro