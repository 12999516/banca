using static System.Console;

namespace es
{
    internal class Ccliente
    {
        int idgruppo;
        bool metallo;
        SemaphoreSlim sem_gruppo;
        SemaphoreSlim sem_banca;
        CancellationToken cts;
        public int Stato { get; set; } // 0: Attesa Ingresso, 1: In Cabina, 2: In Banca, 3: Attesa Uscita, 4: Uscito

        public Ccliente()
        {
            this.idgruppo = 0;
            this.metallo = false;
            this.Stato = 0;
        }

        public Ccliente(int idgruppo, bool metallo, SemaphoreSlim sem_gruppo, SemaphoreSlim sem_banca, CancellationToken cts)
        {
            this.idgruppo = idgruppo;
            this.metallo = metallo;
            this.sem_gruppo = sem_gruppo;
            this.sem_banca = sem_banca;
            this.cts = cts;
            this.Stato = 0;
        }

        private async Task entra_banca()         
        {
            try
            {
                // Attesa per entrare nella cabina (1) - Usa semaforo di gruppo
                await sem_gruppo.WaitAsync(cts);
                this.Stato = 1; // In Cabina
                WriteLine($"Cliente del gruppo {idgruppo} è entrato nella cabina (Controllo Metalli in corso)");

                // Attesa per entrare in banca dopo controllo (2) - Usa semaforo camera blindata
                await sem_banca.WaitAsync(cts);
                this.Stato = 2; // In Banca
                WriteLine($"Cliente del gruppo {idgruppo} è entrato nella camera blindata (Nessun metallo)");
            }
            catch (OperationCanceledException)
            {
                WriteLine($"Cliente del gruppo {idgruppo} espulso dalla cabina (METALLO RILEVATO!)");
                this.Stato = 4; // Espulso / Terminato
                throw; // Rilancia per interrompere il flusso
            }
        }

        public async Task fai_operazione()
        {
            bool entrato = false;
            while (!entrato)
            {
                try
                {
                    await entra_banca();
                    entrato = true;
                }
                catch (OperationCanceledException)
                {
                    // Riprova con un nuovo "gruppo" o lo stesso, ma in questo caso simuliamo che esca e riprovi
                    // Il Program gestirà il reset del token
                    return; // Esce dal task corrente, il Program potrebbe doverlo gestire se vuole che riprovi
                    // La consegna dice "accetta in ingresso un nuovo gruppo". 
                    // Quindi questo task termina.
                }
            }

            WriteLine($"Cliente del gruppo {idgruppo} sta facendo operazioni in banca...");
            await Task.Delay(2000); // Simulazione operazione
            
            this.Stato = 3; // Pronto per uscire
            await esci_dalla_banca();
            this.Stato = 4; // Finito
        }

        private async Task esci_dalla_banca()
        {
            // Attesa per entrare nella cabina per uscire (3)
            await sem_gruppo.WaitAsync(); // L'uscita usa la cabina, quindi il semaforo di gruppo (unica risorsa di "ingresso" alla cabina)
            WriteLine($"Cliente del gruppo {idgruppo} entra nella cabina ed esce dalla banca (4)");
            // Uscita effettiva, rilasciamo sem_gruppo? 
            // No, il release viene fatto dal Program o implicitamente gestito. 
            // In realtà se il Program fa release(N) per farli entrare, loro consumano 1 token.
            // Una volta usciti, dovremmo rilasciare qualcosa? 
            // Il semaforo controlla l'accesso. Una volta consumato, siamo dentro.
            // Se usciamo dalla cabina verso l'esterno, liberiamo la cabina.
            // Ma il semaforo è gestito dal Program per dare il permesso.
        }

        public int dammi_idgruppo()
        {
            return this.idgruppo;
        }

        public bool dimmi_metallo()
        {
            return this.metallo;
        }
    }
}
