using static System.Console;

namespace es
{
    internal class Program
    {
        static List<Ccliente> lista_clienti = new List<Ccliente>();
        static long postiCabina = 5; 
        static long postiBanca = 5; 

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
            int gruppoIngresso = 0;
            int gruppoUscita = 0;

            for (int i = 0; i < num_gruppi; i++)
            {
                sem_gruppi[i] = new SemaphoreSlim(0);
                token_Vari[i] = new CancellationTokenSource();
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

            for(int i = 0; i < 5; i++)
            {
                WriteLine($"Gruppo {i} ha {clientidelgruppo(i)} clienti.");
            }

            for(int i = 0; i < lista_clienti.Count; i++)
            {
                lista_task.Add(lista_clienti[i].fai_operazione());
            }   
            
            while (lista_clienti.Count > 0)
            {

                if (Interlocked.Read(ref postiCabina) > 0)
                {
                    if (gruppoIngresso < num_gruppi)
                    {
                        int numeroMembri = clientidelgruppo(gruppoIngresso);
                        
                        bool haMetallo = hametallo(gruppoIngresso);

                        if (haMetallo) 
                        {
                            WriteLine($"gruppo {gruppoIngresso} ha metallo vengono espulsi. controlli non superati");
                            token_Vari[gruppoIngresso].Cancel();
                            rimuoviclientidallalista(gruppoIngresso); 

                            gruppoUscita++;
                        }
                        else
                        {
                            WriteLine($"gruppo {gruppoIngresso} non ha metallo possono entrare in banca");
                            
                            sem_gruppi[gruppoIngresso].Release(numeroMembri); 
                            camera_blindata.Release(numeroMembri);
                            
                            
                            lock(lck)
                            {
                                postiCabina = 0;
                                postiBanca = 0;
                            }
                        }

                        gruppoIngresso++;
                    }
                    else
                    {
                       await Task.Delay(250);
                    }
                }
                else
                {
                     
                    if (gruppoUscita < gruppoIngresso) 
                    {
                         int idUscita = gruppoUscita;
                         
                         if (clientidelgruppo(idUscita) > 0)
                         {
                             WriteLine($"gruppo {idUscita} esce dalla banca");
                             rimuoviclientidallalista(idUscita);
                         }

                         gruppoUscita++;

                         lock (lck)
                         {
                            postiCabina = 5;
                            postiBanca = 5;
                         }

                        WriteLine("banca libera per nuovo accesso");
                    }
                }
                
                await Task.Delay(750);
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
