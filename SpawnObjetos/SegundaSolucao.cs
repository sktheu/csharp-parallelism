/* 
  . Paralelismo da Segunda Solução:
  Ambas as Threads rodam o mesmo método, que seleciona e verifica a sobreposição entre os objetos. 
  Porém, aqui distribuimos o processamento entre elas, onde usando a mesma lógica de ter um objeto sendo verificado com   
  os demais, fazemos com que a primeira Thread execute a partir da primeira metade de objetos 
  e a segunda execute a partir da outra metade.
*/
using System;
using System.Threading;
public class SegundaSolucao 
{
  // Variáveis Globais
  // Criação
  private static int MAX_X = 5; // Valor Máximo no eixo X
  private static int MAX_Y = 5; // Valor Máximo no eixo Y
  private static int TOTAL = 10; // Quantidade Total de Objetos
  private static int[] X = new int[TOTAL]; // Array que vai armazenar os valores das abcissas dos Objetos
  private static int[] Y = new int[TOTAL]; // Array que vai armazenar os valores das ordenadas dos Objetos

  // Intervalo de pausa para sincronização das Threads
  private static int PAUSA = 50;

  // Armazena a quantidade de sobreposições
  private static int Quant = 0;
  
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
    
    // Crie a 1ª Thread do Método Sobrepondo
    Thread t1 = new Thread(() => Sobrepondo(0, TOTAL/2));
  
    // Crie a 2ª Thread do Método Sobrepondo
    Thread t2 = new Thread(() => Sobrepondo(TOTAL/2, TOTAL));

    // Comece as duas Threads
    t1.Start();
    t2.Start();

    // Faça com que a Thread Princiapl espere as outras duas Threads Terminarem
    t1.Join();
    t2.Join();
    
    if (Quant == 0) Console.WriteLine("Não houve sobreposições."); // Mostre caso não tiver tido nenhuma sobreposição
  }

  /* Método responsável por selecionar e verificar os objetos que estão se sobrepondo
   var inicio = começo do indice do loop
   var condicao = quando terminar o loop
  */
  private static void Sobrepondo(int inicio, int condicao)
  {
    int ObjA; // objeto principal da verificacao
    int ObjB; // demais objetos
    for (int i = inicio; i < condicao; i++)
    {
      ObjA = i; // Troca o objeto que estamos vendo se está se sobrepondo com os demais
      //Console.WriteLine("ObjA ID {0}", ObjA); //Debug
      for (int j = 0; j < TOTAL; j++) 
      {
        ObjB = j; // Intercala entre os demais objetos
        //Console.WriteLine("ObjB ID {0}", ObjB); //Debug
        
        if (ObjA != ObjB) // Caso não for o mesmo objeto sendo comparado
        {
          if (X[ObjA] == X[ObjB] && Y[ObjA] == Y[ObjB]) // Caso ambos os eixos dos objetos forem iguais
          {
            Console.WriteLine("ID {0} e ID {1} estão se sobrepondo!", ObjA, ObjB); // Mostre que estão se sobrepondo
            Quant++; // Mais uma sobreposição
          }
        }
        
        Thread.Sleep(PAUSA); // Pausa para sincronizar com a outra Thread
      }
    }
  }
}