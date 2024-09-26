class Program
{
    static async Task Main(string[] args)
    {
        while (true)
        {
            await CheckTickets();
            await Task.Delay(TimeSpan.FromSeconds(60));
        }
    }

    public static async Task<string> CheckTickets() 
    {
        string result = "failed";
        string baseUrl = "https://localhost:7299/TicketRaise/SendMail";
        var requestUrl = new HttpRequestMessage(HttpMethod.Get, baseUrl);
        var client = new HttpClient();

        var response = await client.SendAsync(requestUrl);
        response.EnsureSuccessStatusCode();
        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            result = await response.Content.ReadAsStringAsync();
            Console.WriteLine(result);
        }

        return result;
    }
}
