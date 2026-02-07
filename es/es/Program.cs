using static System.Console;

namespace es
{
    internal class Program
    {
        static List<Ccliente> lista_clienti = new List<Ccliente>();

        static async Task Main(string[] args)
        {
            int max_membri_gruppo = 5;
            int num_gruppi = 5;
            SemaphoreSlim[] sem_gruppi = new SemaphoreSlim[num_gruppi];
            object lck = new object();
            SemaphoreSlim camera_blindata = new SemaphoreSlim(0); 
            List<Task> lista_task = new List<Task>();
            lista_clienti = new List<Ccliente>();
            CancellationTokenSource[] token_Vari = new CancellationTokenSource[num_gruppi];
            List<Task>[] compitiPerGruppo = new List<Task>[num_gruppi];
            Queue<int> codaIngresso = new Queue<int>();
            Queue<int> codaUscita = new Queue<int>();
            bool versoIngresso = true;
            bool bancaOccupata = false;


            for (int i = 0; i < num_gruppi; i++)
            {
                sem_gruppi[i] = new SemaphoreSlim(0);
                token_Vari[i] = new CancellationTokenSource();
                compitiPerGruppo[i] = new List<Task>();
            }


            for (int i = 0; i < num_gruppi; i++)
            {
                Random rnd = new Random(Environment.TickCount + i);
                int q_C_G = rnd.Next(1, max_membri_gruppo + 1); 
                for(int j = 0; j < q_C_G; j++)
                {
                    bool metallo = rnd.Next(0, 21) == 1;
                    lista_clienti.Add(new Ccliente(i, j,  metallo, sem_gruppi[i], camera_blindata, token_Vari[i].Token));
                }
            }


            for (int i = 0; i < num_gruppi; i++)
            {
                codaIngresso.Enqueue(i);
            }


            for(int i = 0; i < num_gruppi; i++)
            {
                WriteLine($"gruppo {i} ha {clientidelgruppo(i)} clienti");
            }


            for(int i = 0; i < lista_clienti.Count; i++)
            {
                Task t = lista_clienti[i].fai_operazione();
                lista_task.Add(t);
                compitiPerGruppo[lista_clienti[i].dammi_idgruppo()].Add(t);
            }   


            while (lista_clienti.Count > 0)
            {
                if (versoIngresso)
                {
                    if (!bancaOccupata && codaIngresso.Count > 0)
                    {
                        int idGruppo = codaIngresso.Peek();
                        int numeroMembri = clientidelgruppo(idGruppo);
                        bool metallo = hametallo(idGruppo);

                        if (metallo)
                        {
                            WriteLine($"gruppo {idGruppo} ha metallo e viene espulso");
                            token_Vari[idGruppo].Cancel();
                            rimuoviclientidallalista(idGruppo);
                            codaIngresso.Dequeue();
                        }
                        else
                        {
                            WriteLine($"gruppo {idGruppo} non ha metallo, entra in banca");
                            sem_gruppi[idGruppo].Release(numeroMembri);
                            camera_blindata.Release(numeroMembri);
                            
                            codaIngresso.Dequeue();
                            codaUscita.Enqueue(idGruppo);
                            bancaOccupata = true;
                        }
                    }

                    versoIngresso = false;
                }
                else
                {
                    if (bancaOccupata && codaUscita.Count > 0)
                    {
                        int idUscita = codaUscita.Peek();
                        WriteLine($"aspettando che il gruppo {idUscita} finisca le operazioni");

                        await Task.WhenAll(compitiPerGruppo[idUscita]);

                        WriteLine($"gruppo {idUscita} ha finito ed esce dalla banca");
                        rimuoviclientidallalista(idUscita);
                        codaUscita.Dequeue();
                        bancaOccupata = false;
                        WriteLine("banca libera per nuovo accesso");
                    }
                    
                    versoIngresso = true;
                }
                
                await Task.Delay(250);
            }

            await Task.WhenAll(lista_task);
            WriteLine("finiti i gruppo la banca chiude");
        }

        static bool hametallo(int idGruppo)
        {
            for(int i = 0; i < lista_clienti.Count; i ++)
            {
                if (lista_clienti[i].dammi_idgruppo() == idGruppo && lista_clienti[i].dimmi_metallo())
                {
                    return true;
                }
            }
            return false;
        }

        static int clientidelgruppo(int idGruppo)
        {
            int count = 0;

            for(int i = 0; i < lista_clienti.Count; i++)
            {
                if (lista_clienti[i].dammi_idgruppo() == idGruppo)
                {
                    count++;
                }
            }
            return count;
        }

        static void rimuoviclientidallalista(int idGruppo)
        {
             for(int i = lista_clienti.Count - 1; i >= 0; i--)
             {
                 if (lista_clienti[i].dammi_idgruppo() == idGruppo)
                 {
                     lista_clienti.RemoveAt(i);
                 }
             }
        }
    }
}
