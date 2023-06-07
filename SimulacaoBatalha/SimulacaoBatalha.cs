using System;
using System.Threading;

#region Classe Jogo
public class Jogo
{
    // Dimensões do Mapa
    public static int larguraMapa = 36;
    public static int alturaMapa = 10;

    // Matriz 2D que representa o Mapa
    public static int[,] mapa = new int[alturaMapa, larguraMapa];

    // Armazena a Instância da Classe Character
    public static Character character;

    // Vetor que vai armazenar as Instâncias da Classe Enemy
    private static int quantidadeEnemies = 4;
    private static Enemy[] enemies;

    // Armazena a Instância da Classe Heart
    private static Heart heart;

    // Armazena a pontuação atual
    public static int pontuacaoAtual = 0;

    // Instância da Classe Random para gerar números aleatórios
    private static Random rand = new Random();

    // Threads Character
    private static Thread inputThread, atualizarCharThread;

    // Semáforo
    private static readonly object lockObj = new object();

    // Método Principal
    public static void Main(string[] args)
    {
        // Configure o Jogo
        ConfigurarJogo();

        // Game Loop
        while (true)
        {
            // Crie uma Thread para verificação de input do Character
            inputThread = new Thread(CharacterInput);
            inputThread.Start();

            // Crie uma Thread para Atualização da Posição do Character no Mapa
            atualizarCharThread = new Thread(() => character.AtualizarCharacter());
            atualizarCharThread.Start();

            // Espere a Atualização da posição do Character
            atualizarCharThread.Join();

            /* Adicione à ThreadPool as atualizações das instâncias de Enemy e 
               as detecções de colisões entre o Character e as instâncias de Enemy 

                Com o uso da ThreadPool, evitamos o desperdício de processamento na criação 
                e eliminação de threads que é ocorrido na estratégia inicial
            */
            for (int i = 0; i < enemies.Length; i++)
            {
                ThreadPool.QueueUserWorkItem(AtualizarEnemy, i);
                ThreadPool.QueueUserWorkItem(VerificarGameOver, i);
            }

            // Adicione à ThreadPool a detecção de colisão entre o Character e o Objetivo
            ThreadPool.QueueUserWorkItem(VerificarGameWin, heart);

            // Renderizar o Objetivo no Mapa
            heart.RenderizarHeart();

            // Mostrar Mapa
            RenderizarMapa();

            // Mostrar Placar
            RenderizarPlacar();

            // Intervalo de um frame e outro no Game Loop
            Thread.Sleep(1000);
        }
    }

    // Método Responsável pela Deteccção de Input para a movimentação da instância do Character
    private static void CharacterInput()
    {
        // Leitura de Input
        string input = Console.ReadLine().ToUpper();

        // Caso foi digitado algo, verifique se é um input válido
        if (input.Length != 0)
        {
            // Altere a posição do cursor, para que o input digitado não modifique a renderização do mapa
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.BufferWidth));
            Console.SetCursorPosition(0, Console.CursorTop);

            // Irão armazenar as direções do movimento do Character no frame atual
            int dirX = 0;
            int dirY = 0;

            // Verifique se o player quer se mover na horizontal
            if (input.Contains("D")) // Direita
            {
                dirX = 1;
                Console.Write("→");
            }
            else if (input.Contains("A"))  // Esquerda
            {
                dirX = -1;
                Console.Write("<-");
            }

            // Verifique se o player quer se mover na vertical
            if (input.Contains("W")) // Cima
            {
                dirY = -1;
                Console.Write("↑");
            }
            else if (input.Contains("S")) // Baixo
            {
                dirY = 1;
                Console.Write("↓");
            }

            // Aplique o movimento com base nas direções dos inputs inseridos
            character.Mover(dirY, dirX);

            //Thread.Sleep(200);
        }
    }

    // Método Responsável pela Renderização do Mapa, mostrando o Character e Enemies
    private static void RenderizarMapa()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("   ----------------------------");
        Console.WriteLine("   |404 Studios© / Heart Chase|");
        Console.WriteLine("   ----------------------------");
        for (int y = 0; y < alturaMapa; y++) // Eixo Y do Mapa
        {
            for (int x = 0; x < larguraMapa; x++) // Eixo X do Mapa
            {
                if (mapa[y, x] == 0) // Caso não houver nenhuma entidade
                {
                    Console.ForegroundColor = ConsoleColor.Green; // Altere a cor do Console
                    Console.Write("░"); // Desenhe o ASCII que representa o chão
                }
                else
                {
                    int entidade = mapa[y, x];

                    if (entidade == 1)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow; // Altere a cor do Console
                        Console.Write("☻"); // Desenhe o ASCII que representa o Character
                    }
                    else if (entidade == 2)
                    {
                        Console.ForegroundColor = ConsoleColor.Red; // Altere a cor do Console
                        Console.Write("Ø"); // Desenhe o ASCII que representa o Enemy
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkMagenta; // Altere a cor do Console
                        Console.Write("♥"); // Desenhe o ASCII que representa o Objetivo
                    }

                    mapa[y, x] = 0; // Limpe a posição para o próximo Frame
                }

            }
            Console.WriteLine();
        }
    }

    // Método responsável pela configuração inicial / restart do jogo
    public static void ConfigurarJogo(bool pontuou = false)
    {
        // Caso não tiver pontuado, reinicie a pontuação
        if (!pontuou) pontuacaoAtual = 0;

        // Atribua uma nova instância da classe Character
        character = new Character(rand.Next(0, alturaMapa), rand.Next(0, larguraMapa));

        // Atribua novas instâncias da classe Enemy
        enemies = new Enemy[quantidadeEnemies];
        for (int i = 0; i < enemies.Length; i++)
        {

            enemies[i] = new Enemy(rand.Next(0, alturaMapa), rand.Next(0, larguraMapa));

            // Altere até que não seja a mesma posição do Character
            while (enemies[i].Y == character.Y && enemies[i].X == character.X)
            {
                enemies[i].Y = rand.Next(0, alturaMapa);
                enemies[i].X = rand.Next(0, larguraMapa);
            }
        }

        // Atribua uma nova instância da classe Heart
        heart = new Heart(rand.Next(0, alturaMapa), rand.Next(0, larguraMapa));
    }

    // Método responsável por renderizar o placar contendo: pontuacao, posicao do character e inputs
    private static void RenderizarPlacar()
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("------------------------------------");
        Console.WriteLine("|♥: {0} |  ☻ ({1}, {2})  | WASD: MOVER  |", pontuacaoAtual, character.X, character.Y);
        Console.WriteLine("------------------------------------");
    }

    /* Método que será adicionado a ThreadPool, onde verifica se há algum Enemy colidindo com o Character
       Há o uso de semáforo, para garantir acesso seguro na reposição das entidades do jogo */
    private static void VerificarGameOver(object id)
    {
        lock (lockObj)
        {
            character.DetectarColisaoEnemy(enemies[(int)id]);
        }
    }

    /* Método que será adicionado a ThreadPool, onde verifica a colisão entre o Character e o Objetivo
        Há o uso de semáforo, para garantir acesso seguro na reposição das entidades do jogo */
    private static void VerificarGameWin(object h)
    {
        lock (lockObj)
        {
            character.DetectarColisaoHeart((Heart)h);
        }
    }

    /* Método que será adicionado a ThreadPool, onde atualiza a posição e renderiza a instância de Enemy
       Há o uso de semáforo, para garantir acesso seguro a instância de Enemy */
    private static void AtualizarEnemy(object id)
    {
        lock (lockObj)
        {
            int i = (int)id;
            enemies[i].Mover();
            enemies[i].Renderizar();
        }
    }
}
#endregion Fim Classe Jogo

#region Classe Character
public class Character
{
    // Atributos Character
    private int _y;
    private int _x;

    // Propriedades Character
    public int Y
    {
        get { return _y; }
        set
        {   // Tratamento Eixo Y
            if (value <= 0)
                _y = 0;
            else if (value >= Jogo.alturaMapa)
                _y = Jogo.alturaMapa - 1;
            else
                _y = value;
        }
    }

    public int X
    {
        get { return _x; }
        set
        {   // Tratamento Eixo X
            if (value <= 0)
                _x = 0;
            else if (value >= Jogo.larguraMapa)
                _x = Jogo.larguraMapa - 1;
            else
                _x = value;
        }
    }

    // Método Construtor para configurar a posição inicial da instância da Classe Character
    public Character(int y, int x)
    {
        this.Y = y;
        this.X = x;
    }

    // Método para movimentar o Character
    public void Mover(int direcaoY, int direcaoX)
    {
        Y += direcaoY;
        X += direcaoX;
    }

    // Método para atualizar a posição do Character no Mapa do Jogo
    public void AtualizarCharacter()
    {
        Jogo.mapa[Y, X] = 1;
    }

    // Método para verificar se há colisão com determinado inimigo
    public void DetectarColisaoEnemy(Enemy enemy)
    {
        // Caso houver colisão reconfigure o Jogo
        if (this.Y == enemy.Y && this.X == enemy.X)
            Jogo.ConfigurarJogo();
    }

    // Método para verificar se há colisão com o objetivo
    public void DetectarColisaoHeart(Heart heart)
    {
        // Caso houver colisão reconfigure o Jogo e aumente a pontuação
        if (this.Y == heart.Y && this.X == heart.X)
        {
            Jogo.pontuacaoAtual++;
            Jogo.ConfigurarJogo(true);
        }
    }
}
#endregion Fim Classe Character

#region Classe Enemy
public class Enemy
{
    // Atributos da Classe Enemy 
    private int _y;
    private int _x;

    // Propriedades da Classe Enemy
    public int Y
    {
        get { return _y; }
        set
        {   // Tratamento Eixo Y
            if (value <= 0)
                _y = 0;
            else if (value >= Jogo.alturaMapa)
                _y = Jogo.alturaMapa - 1;
            else
                _y = value;
        }
    }

    public int X
    {
        get { return _x; }
        set
        {   // Tratamento Eixo X
            if (value <= 0)
                _x = 0;
            else if (value >= Jogo.larguraMapa)
                _x = Jogo.larguraMapa - 1;
            else
                _x = value;
        }
    }

    private static Random rand = new Random(); // objeto Random compartilhado entre todas as instâncias da Classe Enemy

    // Método Construtor para configurar a posição inicial da instância da Classe Enemy
    public Enemy(int y, int x)
    {
        this.Y = y;
        this.X = x;
    }

    // Método para movimentar o Enemy
    public void Mover()
    {
        /* Código de Movimentação Simples
        // 0 == Não vai Mover, 1 == Mover para cima, -1 == Mover para baixo
        int dirY = rand.Next(-1, 2);
        Y += dirY;

        // 0 == Não vai Mover, 1 == Mover para direita, -1 == Mover para esquerda
        int dirX = rand.Next(-1, 2);
        X += dirX;
        */

        // Código para perseguir o Player
        // Chance de se mover no frame atual
        if (rand.Next(100) < 30)
        {
            // Calcule a direção
            int dirY = Math.Sign(Jogo.character.Y - this.Y);
            int dirX = Math.Sign(Jogo.character.X - this.X);

            // Aplique o Movimento
            Y += dirY;
            X += dirX;
        }
    }

    // Método para renderizar o enemy no Mapa do Jogo
    public void Renderizar()
    {
        // Atualize a posição desse Enemy no Mapa do Jogo
        Jogo.mapa[Y, X] = 2;
    }
}
#endregion Fim Classe Enemy

#region Classe Heart
public class Heart
{
    // Atributos da Classe Heart
    public int Y;
    public int X;

    // Método Construtor para configurar a posição da instância da Classe Heart
    public Heart(int y, int x)
    {
        this.Y = y;
        this.X = x;
    }

    // Método responsável por renderizar a posição no mapa aonde está o objetivo
    public void RenderizarHeart()
    {
        Jogo.mapa[Y, X] = 3;
    }
}
#endregion Fim Classe Heart