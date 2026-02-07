using static System.Console;

namespace es
{
    internal class Ccliente
    {
        int idgruppo;
        int idcliente;
        bool metallo;
        SemaphoreSlim sem_gruppo;
        SemaphoreSlim sem_banca;
        CancellationToken cts;
        Random rnd;

        public Ccliente()
        {
            idcliente = 0;
            idgruppo = -1;
            metallo = false;
            sem_banca = new SemaphoreSlim(0, 0);
            sem_gruppo = new SemaphoreSlim(0, 0);
            cts = CancellationToken.None;
        }

        public Ccliente(int idgruppo, int idcliente, bool metallo, SemaphoreSlim sem_gruppo, SemaphoreSlim sem_banca, CancellationToken cts)
        {
            this.idgruppo = idgruppo;
            this.idcliente = idcliente + 1;
            this.metallo = metallo;
            this.sem_gruppo = sem_gruppo;
            this.sem_banca = sem_banca;
            this.cts = cts;
            rnd = new Random(Environment.TickCount);
        }

        private async Task entracabina()
        {
            Task.Delay(rnd.Next(100, 201)).Wait();
            await sem_gruppo.WaitAsync(cts);
            WriteLine($"cliente {idcliente} del gruppo {idgruppo} è entrato nella cabina");
        }

        private async Task entrabanca()
        {
            Task.Delay(rnd.Next(100, 201)).Wait();
            await sem_banca.WaitAsync(cts);
            WriteLine($"cliente {idcliente} del gruppo {idgruppo} è entrato nella banca.");
        }

        public async Task fai_operazione()
        {
            try
            {
                await entracabina();
                await entrabanca();

                WriteLine($"cliente {idcliente} del gruppo {idgruppo} sta facendo l'operazione");
                await Task.Delay(1000); 
            }
            catch (OperationCanceledException)
            {
                WriteLine($"cliente {idcliente} del gruppo {idgruppo} espulso insieme al suo gruppo perché rilevato metallo");
            }
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
