using static System.Console;

namespace es
{
    internal class Program
    {
        static List<Ccliente> lista_clienti;
        static int cabinCapacity = 5; // N elements

        static async Task Main(string[] args)
        {
            int max_clienti = 5;
            SemaphoreSlim[] gruppi = new SemaphoreSlim[5];
            SemaphoreSlim camera_blindata = new SemaphoreSlim(0); 
            List<Task> lista_task = new List<Task>();
            lista_clienti = new List<Ccliente>();
            CancellationTokenSource[] token_Vari = new CancellationTokenSource[5];

            for (int i = 0; i < gruppi.Length; i++)
            {
                gruppi[i] = new SemaphoreSlim(0);
                token_Vari[i] = new CancellationTokenSource();
            }

            for (int i = 0; i < 5; i++)
            {
                Random rnd = new Random(Environment.TickCount + i);
                int q_C_G = rnd.Next(1, max_clienti + 1); 
                for(int j = 0; j < q_C_G; j++)
                {
                    bool metallo = rnd.Next(0, 2) == 1;
                    lista_clienti.Add(new Ccliente(i, metallo, gruppi[i], camera_blindata, token_Vari[i].Token));
                }
            }

            for(int i = 0; i < lista_clienti.Count; i++)
            {
                lista_task.Add(lista_clienti[i].fai_operazione());
            }

            bool cabinaLibera = true;
            int gruppoInCabina = -1; 
            bool inUscita = false; 

            while (lista_clienti.Count > 0)
            {
                await Task.Delay(100);
                
                if (cabinaLibera)
                {

                    int gruppoUscita = TrovaGruppoPronto(3);
                    if (gruppoUscita != -1)
                    {
                         WriteLine($"Apertura cabina verso l'interno per gruppo {gruppoUscita} (Uscita).");
                         inUscita = true;
                         gruppoInCabina = gruppoUscita;
                         cabinaLibera = false;
                         int count = ContaClientiGruppo(gruppoUscita);
                         gruppi[gruppoUscita].Release(count);
                    }
                    else
                    {
                        int gruppoIngresso = TrovaGruppoPronto(0);
                        if (gruppoIngresso != -1)
                        {
                            WriteLine($"Apertura cabina verso l'esterno per gruppo {gruppoIngresso} (Ingresso).");
                            inUscita = false;
                            gruppoInCabina = gruppoIngresso;
                            cabinaLibera = false;
                            int count = ContaClientiGruppo(gruppoIngresso);
                            gruppi[gruppoIngresso].Release(count);
                        }
                    }
                }
                else
                {
                    
                    if (inUscita)
                    {
                        if (TuttiNelStato(gruppoInCabina, 4))
                        {
                            WriteLine($"Gruppo {gruppoInCabina} uscito definitivamente.");
                            cabinaLibera = true;
                            gruppoInCabina = -1;
                            
                            RimuoviClientiStato(4);
                        }
                    }
                    else
                    {
                        if (TuttiNelStato(gruppoInCabina, 1))
                        {
                            WriteLine($"Gruppo {gruppoInCabina} in cabina. Controllo metalli...");
                            if (GruppoHaMetallo(gruppoInCabina))
                            {
                                WriteLine($"METALLO RILEVATO nel gruppo {gruppoInCabina}! Espulsione.");
                                token_Vari[gruppoInCabina].Cancel();
                                
                                cabinaLibera = true;
                                gruppoInCabina = -1;
                                
                                RimuoviClientiGruppo(gruppoInCabina);
                            }
                            else
                            {
                                WriteLine($"Gruppo {gruppoInCabina} pulito. Entrano nella camera blindata.");
                                int count = ContaClientiGruppo(gruppoInCabina);
                                camera_blindata.Release(count);
                                cabinaLibera = true;
                                gruppoInCabina = -1;
                            }
                        }
                    }
                }
            }
            
            WriteLine("Simulazione terminata.");
        }

        static void RimuoviClientiStato(int stato)
        {
            for (int i = lista_clienti.Count - 1; i >= 0; i--)
            {
                if (lista_clienti[i].Stato == stato)
                {
                    lista_clienti.RemoveAt(i);
                }
            }
        }

        static void RimuoviClientiGruppo(int idGruppo)
        {
             for (int i = lista_clienti.Count - 1; i >= 0; i--)
            {
                if (lista_clienti[i].dammi_idgruppo() == idGruppo)
                {
                    lista_clienti.RemoveAt(i);
                }
            }
        }

        static bool TuttiNelStato(int idGruppo, int statoTarget)
        {
            int totali = 0;
            int corretti = 0;
            for(int i = 0; i < lista_clienti.Count; i++) 
            {
                if (lista_clienti[i].dammi_idgruppo() == idGruppo)
                {
                    totali++;
                    if (lista_clienti[i].Stato == statoTarget)
                    {
                        corretti++;
                    }
                }
            }

            if (totali == 0) return true;
            return totali == corretti;
        }

        static bool GruppoHaMetallo(int idGruppo)
        {
            for (int i = 0; i < lista_clienti.Count; i++)
            {
                if (lista_clienti[i].dammi_idgruppo() == idGruppo && lista_clienti[i].dimmi_metallo())
                {
                    return true;
                }
            }
            return false;
        }

        static int ContaClientiGruppo(int idGruppo)
        {
            int count = 0;
            for (int i = 0; i < lista_clienti.Count; i++)
            {
                if (lista_clienti[i].dammi_idgruppo() == idGruppo)
                {
                    count++;
                }
            }
            return count;
        }

        static int TrovaGruppoPronto(int statoTarget)
        {
            List<int> gruppiIds = new List<int>();
            for (int i = 0; i < lista_clienti.Count; i++)
            {
                int gid = lista_clienti[i].dammi_idgruppo();
                if (!gruppiIds.Contains(gid))
                {
                    gruppiIds.Add(gid);
                }
            }
            gruppiIds.Sort();

            foreach(int gid in gruppiIds)
            {
                if (TuttiNelStato(gid, statoTarget))
                {
                    return gid;
                }
            }
            return -1;
        }
    }
}
