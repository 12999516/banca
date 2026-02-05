using static System.Console;

namespace es
{
    internal class Ccliente
    {
        int idgruppo;
        int idcliente;
        bool metallo;
        SemaphoreSlim sem_gruppo;
        SemaphoreSlim sem_camerablindata;
        CancellationToken cts;

        public Ccliente()
        {
            idcliente = 0;
            idgruppo = -1;
            metallo = false;
            sem_camerablindata = new SemaphoreSlim(0, 0);
            sem_gruppo = new SemaphoreSlim(0, 0);
            cts = CancellationToken.None;
        }

        public Ccliente(int idgruppo, int idcliente, bool metallo, SemaphoreSlim sem_gruppo, SemaphoreSlim sem_camerablindata, CancellationToken cts)
        {
            this.idgruppo = idgruppo;
            this.idcliente = idcliente + 1;
            this.metallo = metallo;
            this.sem_gruppo = sem_gruppo;
            this.sem_camerablindata = sem_camerablindata;
            this.cts = cts;
        }

        private async Task entrabanca()
        {
            await sem_gruppo.WaitAsync(cts);
            WriteLine($"cliente {idcliente} del gruppo {idgruppo} è entrato nella banca");
        }

        private async Task entracabina()
        {
            await sem_camerablindata.WaitAsync(cts);
            WriteLine($"cliente {idcliente} del gruppo {idgruppo} è entrato nella camera blindata.");
        }

        public async Task fai_operazione()
        {
            try
            {
                await entrabanca();
                await entracabina();

                WriteLine($"cliente {idcliente} del gruppo {idgruppo} sta facendo l'operazione");
                await Task.Delay(2000); 
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
