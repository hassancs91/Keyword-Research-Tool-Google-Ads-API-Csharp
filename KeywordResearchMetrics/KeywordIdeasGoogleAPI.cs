using ConsoleTables;
using Google.Ads.GoogleAds;
using Google.Ads.GoogleAds.Lib;
using Google.Ads.GoogleAds.V10.Enums;
using Google.Ads.GoogleAds.V10.Errors;
using Google.Ads.GoogleAds.V10.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;

namespace KeywordResearchMetrics
{
    public class KeywordIdeasGoogleApi
    {
        public void Authenticate()
        {
            const string GOOGLE_ADS_API_SCOPE = "https://www.googleapis.com/auth/adwords";

            Console.WriteLine("This code example creates an OAuth2 refresh token for the " +
                            "Google Ads API .NET Client library. This example works with both web and " +
                            "desktop app OAuth client ID types. To use this application\n" +
                              "1) Follow the instructions on " +
                              "https://developers.google.com/google-ads/api/docs/oauth/cloud-project " +
                              "to generate a new client ID and secret.\n" +
                              "2) Run this application.\n" +
                              "3) Enter the client ID and client secret when prompted and follow the instructions.\n" +
                              "4) Once the output is generated, copy its contents into your App.config " +
                              "file. See https://developers.google.com/google-ads/api/docs/client-libs/dotnet/configuration " +
                              "for other configuration options.\n\n");

            Console.WriteLine("IMPORTANT: For web app clients types, you must add " +
                "'http://127.0.0.1/authorize' to the 'Authorized redirect URIs' list in your " +
                "Google Cloud Console project before running this example to avoid getting a " +
                "redirect_uri_mismatch error. Desktop app client types do not require the " +
                "local redirect to be explicitly configured in the console.\n\n");

            // Accept the client ID from user.
            Console.Write("Enter the client ID: ");
            var clientId = Console.ReadLine();

            // Accept the client ID from user.
            Console.Write("Enter the client secret: ");
            var clientSecret = Console.ReadLine();

            // Load the JSON secrets.
            var secrets = new ClientSecrets()
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            };

            try
            {
                // Authorize the user using desktop flow. GoogleWebAuthorizationBroker creates a
                // web server that listens to a random port at 127.0.0.1 and the /authorize url
                // as loopback url. See https://github.com/googleapis/google-api-dotnet-client/blob/main/Src/Support/Google.Apis.Auth/OAuth2/LocalServerCodeReceiver.cs
                // for details.
                Task<UserCredential> task = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    secrets,
                    new[] { GOOGLE_ADS_API_SCOPE },
                    string.Empty,
                    CancellationToken.None,
                    new NullDataStore()
                );
                var credential = task.Result;

                Console.WriteLine("\nCopy the following content into your App.config file.\n\n" +
                    $"<add key = 'OAuth2Mode' value = 'APPLICATION' />\n" +
                    $"<add key = 'OAuth2ClientId' value = '{clientId}' />\n" +
                    $"<add key = 'OAuth2ClientSecret' value = '{clientSecret}' />\n" +
                    $"<add key = 'OAuth2RefreshToken' value = " +
                    $"'{credential.Token.RefreshToken}' />\n");

                Console.WriteLine("/n" +
                    "<!-- Required for manager accounts only: Specify the login customer -->\n" +
                    "<!-- ID used to authenticate API calls. This will be the customer ID -->\n" +
                    "<!-- of the authenticated manager account. It should be set without -->\n" +
                    "<!-- dashes, for example: 1234567890 instead of 123-456-7890. You can -->\n" +
                    "<!-- also specify this later in code if your application uses -->\n" +
                    "<!-- multiple manager account OAuth pairs. -->\n" +
                    "<add key = 'LoginCustomerId' value = INSERT_LOGIN_CUSTOMER_ID_HERE />/n/n");


                Console.WriteLine("See https://developers.google.com/google-ads/api/docs/client-libs/dotnet/configuration " +
                    "for alternate configuration options.");
                Console.WriteLine("Press <Enter> to continue...");
                Console.ReadLine();
                

            }
            catch (AggregateException)
            {
                Console.WriteLine("An error occurred while authorizing the user.");
            }
        }

        public void Search(GoogleAdsClient client, long customerId, long[] locationIds,
                                long languageId, string[] keywordTexts)
        {
            var keywordPlanIdeaService =
                client.GetService(Services.V10.KeywordPlanIdeaService);

            //Check if keyword is not empty
            if (keywordTexts.Length == 0)
            {
                throw new ArgumentException("At least one keyword is required!");
            }


            var request = new GenerateKeywordIdeasRequest
            {
                CustomerId = customerId.ToString(),
                KeywordSeed = new KeywordSeed()
            };


            request.KeywordSeed.Keywords.AddRange(keywordTexts);


            //Set Locations
            foreach (long locationId in locationIds)
            {
                request.GeoTargetConstants.Add(ResourceNames.GeoTargetConstant(locationId));
            }
            //Set Language
            request.Language = ResourceNames.LanguageConstant(languageId);
            //Set Network
            //Google or GoogleWithPartners
            request.KeywordPlanNetwork = KeywordPlanNetworkEnum.Types.KeywordPlanNetwork.GoogleSearch;

            try
            {
                // Generate keyword ideas based on the specified parameters.
                var response =
                    keywordPlanIdeaService.GenerateKeywordIdeas(request);

                //Create a Console Table
                var table = new ConsoleTable("Keyword", "Search Volume", "Competition","Low CPC Bid", "High CPC Bid");

                // Iterate over the results and add to the table
                foreach (var result in response)
                {
                    var metrics = result.KeywordIdeaMetrics;

                    if (metrics != null)
                    {
                       
                        var lowBid = Convert.ToDecimal(metrics.LowTopOfPageBidMicros) / 1000000;
                        var highBid = Convert.ToDecimal(metrics.LowTopOfPageBidMicros) / 1000000;

                        table.AddRow(result.Text, metrics.AvgMonthlySearches, metrics.CompetitionIndex, "$ " + lowBid, "$ " + highBid);
                    }
                    else
                    {
                        table.AddRow(result.Text, "", "","","");
                    }

                }

                table.Write();
            }
            catch (GoogleAdsException e)
            {
                Console.WriteLine("Failure!");
                Console.WriteLine($"Message: {e.Message}");
                Console.WriteLine($"Failure: {e.Failure}");
                Console.WriteLine($"Request ID: {e.RequestId}");
                throw;
            }
        }

    }
}
