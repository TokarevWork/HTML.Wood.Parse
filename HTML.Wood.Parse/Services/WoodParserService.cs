using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using HTML.Wood.Parse.Models;
using HTML.Wood.Parse.Services.Services;

namespace HTML.Wood.Parse.Services
{
    internal class WoodParserService : IDisposable
    {
        private static int jobIteration = 0;

        private const string requestAddress = "https://lesegais.ru/open-area/graphql";
        private const int RowsInPage = 20000;

        private readonly TimeSpan TimeIteration = TimeSpan.FromMinutes(10);
        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();
        private readonly DbService _dbService = new DbService();

        private static Random random = new Random();
        private static bool doWork = true;

        public async Task StartJob()
        {
            while (doWork)
            {
                await ParserWork();
                await Task.Delay(TimeIteration);
            }
        }

        private async Task ParserWork()
        {
            Console.WriteLine($"Job iteration {jobIteration} started");

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var total = await JobRetry(GetTotalEntities);

                if (total == 0)
                {
                    Console.WriteLine("Return with total 0");
                    return;
                }

                var pages = GetPageCount(total);

                for (var page = 0; page < pages; page++)
                {
                    var requestResult = await JobRetry(() => GetRequestResult(page));

                    if (requestResult == null)
                    {
                        Console.WriteLine("Return with request result is null");
                        return;
                    }

                    var content = requestResult.Data.SearchReportWoodDeal.Content;

                    for (var iteration = 0; iteration < content.Count; iteration++)
                    {
                        await InsertOrUpdateDataToDb(content, iteration);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine($"Job iteration {jobIteration++} ended on {stopwatch.Elapsed.TotalSeconds:0.00} seconds");
        }

        private int GetPageCount(int total)
        {
            return total % RowsInPage == 0
                ? total / RowsInPage
                : total / RowsInPage + 1;
        }

        private async Task<int> GetTotalEntities()
        {
            var requestQuery = "{\"operationName\": \"SearchReportWoodDealCount\", \"query\": \"query SearchReportWoodDealCount($size: Int!, $number: Int!, $filter: Filter, $orders: [Order!]) {\\n  searchReportWoodDeal(filter: $filter, pageable: {number: $number, size: $size}, orders: $orders) {\\ntotal}\\n}\\n\", \"variables\": {\"size\": 20, \"number\": 0, \"filter\": null}}";
            using (var httpService = new HttpService(requestAddress))
            {
                var result = await httpService.GetRequest<RequestResult>(requestQuery);
                return result.Data.SearchReportWoodDeal.Total;
            }

        }

        private async Task<RequestResult> GetRequestResult(int page)
        {
            var requestQuery = "{\"operationName\": \"SearchReportWoodDeal\",\r\n\"query\": \"query SearchReportWoodDeal($size: Int!, $number: Int!, $filter: Filter, $orders: [Order!]) {\\nsearchReportWoodDeal(filter: $filter, pageable: {number: $number, size: $size}, orders: $orders) {\\ncontent {\\nsellerName\\nsellerInn\\nbuyerName\\nbuyerInn\\nwoodVolumeBuyer\\nwoodVolumeSeller\\ndealDate\\ndealNumber\\n__typename\\n}\\n__typename\\n}\\n}\\n\",\r\n\"variables\": {\"size\":"
                + RowsInPage + ", \"number\": "
                + page + ", \"filter\": null, \"orders\": null}}";
            using (var httpService = new HttpService(requestAddress))
            {
                return await httpService.GetRequest<RequestResult>(requestQuery);
            }
        }

        private async Task InsertOrUpdateDataToDb(List<Content> content, int iteration)
        {
            content[iteration].SellerName = (content[iteration].SellerName is null ? content[iteration].SellerName : content[iteration].SellerName.Replace("'", "''"));
            content[iteration].BuyerName = (content[iteration].BuyerName is null ? content[iteration].BuyerName : content[iteration].BuyerName.Replace("'", "''"));

            var sqlExpression = $@"UPDATE [WoodTransactionsDb].[dbo].[WoodsTransactions] 
                                SET
                                id = CAST(HASHBYTES('MD5', '{content[iteration].DealNumber}') AS UNIQUEIDENTIFIER),
                                declaration_number = '{content[iteration].DealNumber}',
                                seller_name = '{content[iteration].SellerName}',
                                seller_INN = '{content[iteration].SellerInn}',
                                buyer_name = '{content[iteration].BuyerName}',
                                buyer_INN = '{content[iteration].BuyerInn}',
                                transaction_date = '{content[iteration].DealDate}',
                                wood_Volume_Buyer = '{content[iteration].WoodVolumeBuyer}',
                                wood_Volume_Seller = '{content[iteration].WoodVolumeSeller}'
                                WHERE id = CAST(HASHBYTES('MD5', '{content[iteration].DealNumber}') AS UNIQUEIDENTIFIER)
                                IF(@@ROWCOUNT = 0)
                                INSERT INTO[WoodTransactionsDb].[dbo].[WoodsTransactions]
                                (id,
                                declaration_number,
                                seller_name, seller_INN,
                                buyer_name, buyer_INN,
                                transaction_date,
                                wood_Volume_Buyer,
                                wood_Volume_Seller)
                                VALUES
                                (CAST(HASHBYTES('MD5', '{content[iteration].DealNumber}') AS UNIQUEIDENTIFIER),
                                '{content[iteration].DealNumber}',
                                '{content[iteration].SellerName}',
                                '{content[iteration].SellerInn}',
                                '{content[iteration].BuyerName}',
                                '{content[iteration].BuyerInn}',
                                '{content[iteration].DealDate}',
                                '{content[iteration].WoodVolumeBuyer}',
                                '{content[iteration].WoodVolumeSeller}')";

            await _dbService.ExecuteNonQueryAsync(sqlExpression, tokenSource.Token);
        }

        private async Task<Tresult> JobRetry<Tresult>(Func<Task<Tresult>> action)
        {
            var retryCount = 3;

            while (retryCount > 0)
            {
                try
                {
                    return await action.Invoke();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    retryCount--;
                }
                await JobDelay();
            }
            return default;
        }

        private async Task JobDelay()
        {
            var delayFrom = 2000;
            var delayTo = 5000;
            int randomResult = random.Next(delayFrom, delayTo);
            await Task.Delay(randomResult, tokenSource.Token);
        }

        public void Dispose()
        {
            doWork = true;
            tokenSource?.Cancel();
            _dbService?.Dispose();
        }
    }
}
