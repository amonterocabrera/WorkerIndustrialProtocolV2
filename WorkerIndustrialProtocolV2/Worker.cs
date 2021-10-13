using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LazyCache;

namespace WorkerIndustrialProtocolV2
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        //simulate blocks
        IEnumerable<int> squares = (new int[4095]).Select((o, i) => i);
        List<Blocks> blocksList = new List<Blocks>();
        List<Blocks> BlocksCache = new List<Blocks>();

        //create cache
        IAppCache cache = null;
        


        public Worker(ILogger<Worker> logger)
        {
            // Create the cache
            cache = new CachingService();            
            _logger = logger;
            foreach (var item in squares)
            {
                Blocks block = new Blocks();
                block.Id = item;
                block.BufferName = "Buffer-" + item.ToString();
                blocksList.Add(block);

            }

        }


        /// <summary>
        /// Utilizo la clase Worker para configurar que las llamadas sean recurrentes en un lapzo de tiempo definido
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("WorkerIndustrialProtoco:  EndPoint Read() running at: {time}", DateTimeOffset.Now);

                LoadValue();
                await Task.Delay(2000, stoppingToken);
            }
        }


        /// <summary>
        ///  Este metodo se encarga de recibir los valores y consultar la cache sino existe consulta en el origen de datos y lo agrega a la cache para proxima consulta
        /// </summary>
        /// <param name="startValue"></param>
        /// <param name="LengthValue"></param>
        public void Read(int startValue, int LengthValue)
        {
            //recibo los pametros y primero los buzco en la cache si estan lo retorno desde esta de lo contrario consulto la raiz lo retorno y lo agrego a la cache
             BlocksCache = cache.GetOrAdd("get-blocks", () => blocksList.OrderBy(c => c.Id).Where( p => p.Id >= startValue).Take(LengthValue)).ToList();
             BlocksCache.ForEach(i => Console.Write("{0}\t", i.BufferName ,"\n"));

        }


        
       /// <summary>
       /// recibe los valores de la consulta
       /// </summary>
        public void LoadValue()
        {
            int start, length;

            Console.Write("Present initial value: ");
            start = Convert.ToInt32(Console.ReadLine());

            Console.Write("Present Length value: ");
            length = Convert.ToInt32(Console.ReadLine());

            Read(start, length);

        }


        public class Blocks
        {
            public int Id { get; set; }
            public string BufferName { get; set; }
        }


    }
}
