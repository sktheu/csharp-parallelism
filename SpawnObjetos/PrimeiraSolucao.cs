/* Grupo 404 - SMAUG:
  Caike Grion dos Santos
  João Pedro Queiroz de Melo
  João Vitor dos Reis Domingues
  Lucas Neves Timar
  Lucas Proetti Quadros
  Matheus Santos Duca
  -------------------
  . Paralelismo da Primeira Solução:
  Dividir as ações em 2 Threads: A primeira Thread irá cuidar de selecionar as coordenadas dos objetos
  enquanto a segunda irá verificar a sobreposição entre os objetos selecionados pela primeira.
*/
using System;
using System.Threading;
public class PrimeiraSolucao
{
  // Variáveis Globais
  // Criação
  private static int MAX_X = 5; // Valor Máximo no eixo X
  private static int MAX_Y = 5; // Valor Máximo no eixo Y
  private static int TOTAL = 10; // Quantidade Total de Objetos
  private static int[] X = new int[TOTAL]; // Array que vai armazenar os valores das abcissas dos Objetos
  private static int[] Y = new int[TOTAL]; // Array que vai armazenar os valores das ordenadas dos Objetos
  
  // Sobreposição
  private static int ObjA; // Objeto atual que será verificado com base nos outros (Primeiro loop do Percorra)
  private static int ObjB; // Armazena os outros objetos da verificação, que irão se intercalar (Segundo loop do Percorra)

  // Controla o loop da Thread que irá verificar as sobreposições
  private static bool Percorrendo = true;

  // Intervalo de pausa para sincronização das Threads
  private static int PAUSA = 100;
  
  public static void Main (string[] args) 
  {
    Random r = new Random(); // Crie uma instância da classe Random, para conseguir gerar os valores int aleatórios
    Console.WriteLine("------------------------");
    Console.WriteLine("Spawnando {0} Objetos:", TOTAL);
    // Usando um For, preencha o valor X e Y de cada objeto conforme o Total que temos
    for (int i = 0; i < TOTAL; i++)
    {
      X[i] = r.Next(0, MAX_X+1); // Usando o objeto r, gere um valor aleatório no eixo X para o objeto atual
      Y[i] = r.Next(0, MAX_Y+1); // Usando o objeto r, gere um valor aleatório no eixo Y para o objeto atual

      Console.WriteLine("ID {0} Posição = ({1}, {2})", i, X[i], Y[i]); // Debug
    }
    Console.WriteLine("------------------------");
    Console.WriteLine("Sobreposições:");
  
    // Crie a Thread do Método Percorra
    Thread tP = new Thread(Percorra);
  
    // Crie a Thread do Método EstaSobrepondo
    Thread tS = new Thread(EstaSobrepondo);

    // Comece as duas Threads
    tP.Start();
    tS.Start();
  }

  // Método responsável por selecionar os objetos que irão ser verificados
  private static void Percorra()
  {
    Percorrendo = true;
    for (int i = 0; i < TOTAL; i++)
    {
      ObjA = i; // Troca o objeto que estamos vendo se está se sobrepondo com os demais
      //Console.WriteLine("ObjA ID {0}", ObjA); //Debug
      for (int j = 0; j < TOTAL; j++) 
      {
        ObjB = j; // Intercala entre os demais objetos
        //Console.WriteLine("ObjB ID {0}", ObjB); //Debug

        Thread.Sleep(PAUSA); // Pausa para sincronizar com a Thread do EstaSobrepondo
      }
    }
    Percorrendo = false;
  }

  // Método que realiza a verificação de sobreposição entre os Objetos atualmente selecionados
  private static void EstaSobrepondo()
  {
    int quant = 0; // Armazena a Quantidade de sobreposições
    while(Percorrendo)
    {
      if (ObjA != ObjB) // Caso não for o mesmo objeto sendo comparado
      {
        if (X[ObjA] == X[ObjB] && Y[ObjA] == Y[ObjB]) // Caso ambos os eixos dos objetos forem iguais
        {
          Console.WriteLine("ID {0} e ID {1} estão se sobrepondo!", ObjA, ObjB); // Mostre que estão se sobrepondo
          quant++; // Mais uma sobreposição
        }
      }
      Thread.Sleep(PAUSA); // Pausa para sincronizar com a Thread do Percorra
    }

    if (quant == 0) Console.WriteLine("Não houve sobreposições."); // Mostre caso não tiver tido nenhuma sobreposição
  }
}