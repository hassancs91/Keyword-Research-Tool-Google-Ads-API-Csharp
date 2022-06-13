using Google.Ads.GoogleAds.Lib;
using KeywordResearchMetrics;

Console.WriteLine("Free Google Keyword Tool--------------");
var keywordsApi = new KeywordIdeasGoogleApi();
Console.WriteLine("Please Choose a Task:");
Console.WriteLine("1. Authenticate");
Console.WriteLine("2. Perform Keyword Research");

var option = Console.ReadLine();

if (option == "1")
{
    keywordsApi.Authenticate();
}
else
{
    Console.WriteLine("\nPlease Enter a Keyword");

    var keyword = Console.ReadLine();

    // The list of user input keywords.
    var keywordTexts = new[] { "" + keyword + "" };

    // The customer ID for which the call is made.
    var customerId = long.Parse("");


    // Location criteria IDs. For example, specify 21167 for New York. For more
    // information on determining this value, see
    // https://developers.google.com/google-ads/api/reference/data/geotargets.
    // Add more items to the array as desired.
    var locationIds = new[]
    {
        long.Parse("21167")
    };


    // A language criterion ID. For example, specify 1000 for English. For more
    //         information on determining this value, see
    //         https://developers.google.com/google-ads/api/reference/data/codes-formats#languages.
    var languageId = long.Parse("1000");



    var client = new GoogleAdsClient();
    keywordsApi.Search(client, customerId,
        locationIds.ToArray(), languageId, keywordTexts.ToArray());

}



Console.ReadKey();




