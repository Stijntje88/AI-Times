using AI_Times.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using OpenAI.Chat;
using AI_Times.Data.Models;
using System.Security.Cryptography;

namespace AI_Times.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<NewspaperArticle> Articles { get; set; }
        

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql(
                    "server=localhost;port=3306;database=AI_Times;user=root;password=;",
                    new MySqlServerVersion(new Version(8, 0, 30))
                );
            }
        }

        private static string? GetOpenAiApiKey()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                var envFilePath = Path.Combine(directory.FullName, ".env");
                if (File.Exists(envFilePath))
                {
                    foreach (var line in File.ReadAllLines(envFilePath))
                    {
                        if (line.StartsWith("OPENAI_API_KEY=", StringComparison.OrdinalIgnoreCase))
                        {
                            var value = line.Substring("OPENAI_API_KEY=".Length).Trim().Trim('"');
                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                return value;
                            }
                        }
                    }
                }

                directory = directory.Parent;
            }

            return Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY", EnvironmentVariableTarget.User)
                ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY", EnvironmentVariableTarget.Machine);
        }

        private static string EnsureDistinctImageUrl(string? primaryUrl, string? secondaryUrl, string fallbackUrl)
        {
            var normalizedPrimary = NormalizeImageUrl(primaryUrl);
            var normalizedSecondary = NormalizeImageUrl(secondaryUrl);

            if (string.IsNullOrWhiteSpace(normalizedSecondary) ||
                string.Equals(normalizedPrimary, normalizedSecondary, StringComparison.OrdinalIgnoreCase))
            {
                return AddUniqueLockParameter(fallbackUrl);
            }

            return AddUniqueLockParameter(secondaryUrl!);
        }

        private static string NormalizeImageUrl(string? url)
        {
            return (url ?? string.Empty).Trim();
        }

        private static string AddUniqueLockParameter(string url)
        {
            var normalizedUrl = NormalizeImageUrl(url);
            if (string.IsNullOrWhiteSpace(normalizedUrl) ||
                !normalizedUrl.Contains("loremflickr.com", StringComparison.OrdinalIgnoreCase))
            {
                return normalizedUrl;
            }

            var separator = normalizedUrl.Contains('?') ? "&" : "?";
            var lockValue = RandomNumberGenerator.GetInt32(1, int.MaxValue);
            return $"{normalizedUrl}{separator}lock={lockValue}";
        }

        public static async Task GenerateDailyNewsAsync(AppDbContext db)
        {
            try
            {
                Console.WriteLine("Starting GenerateDailyNewsAsync...");

                // Only generate a new article if we do not already have one for today
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                var hasArticleForToday = await db.Articles
                    .AsNoTracking()
                    .AnyAsync(a => a.PublishDate >= today && a.PublishDate < tomorrow);

                if (hasArticleForToday)
                {
                    Console.WriteLine("Article for today already exists. Skipping generation.");
                    return;
                }

                var apiKey = GetOpenAiApiKey();
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new InvalidOperationException("OPENAI_API_KEY not found.");
                }

                var client = new ChatClient("gpt-4o-mini", apiKey);

                Console.WriteLine("Preparing API request...");

                var prompt = @"Write a detailed newspaper article about today's important world news.

ARTICLE REQUIREMENTS:
- Total length: approximately 1000-1200 words
- Title: Compelling and newsworthy headline
- Subtitle1: A subtitle introducing the Introduction section
- Introduction: 200-250 words providing context and introducing the main story
- Subtitle2: A subtitle introducing the Middle section
- Middle: 700-800 words with:
  * Detailed background information
  * Key facts, statistics, and data
  * Expert opinions or quotes
  * Analysis of impact and implications
  * Historical context where relevant
  * Multiple perspectives on the issue
  * IMPORTANT: Separate paragraphs with \n\n (double line breaks)
- Subtitle3: A subtitle introducing the Conclusion section
- Conclusion: 150-200 words summarizing key takeaways and future outlook
- Genre: Choose EXACTLY ONE from: Breaking News, Political News, Business and Economic, International (World), Technology News, Science and Health, Sports News, Entertainment and Culture, Lifestyle and Human-Interest, Investigative Journalism.

IMAGE REQUIREMENTS:
- Provide 2 real, relevant image URLs from LoremFlickr
- Images should be high-quality (1920x1080)
- Use LoremFlickr URLs in this format: https://loremflickr.com/1920/1080/[keywords]
- Choose specific keywords that match your article topic, separated by commas
- Image1: Main topic representation
- Image2: Secondary or related aspect
- Image1 and Image2 must be different URLs with different keywords

RESPONSE FORMAT:
You must respond with ONLY a valid JSON object. Use \n\n for paragraph breaks within sections.

{
""Title"": ""Your compelling article title here"",
""Subtitle1"": ""Subtitle for the Introduction"",
""Introduction"": ""Your 200-250 word introduction. This should hook the reader and provide essential context about what happened, when, and why it matters. Include key details that make this newsworthy and set up the story that follows."",
""Subtitle2"": ""Subtitle for the Middle section"",
""MiddleSection"": ""First paragraph of your main content (150-200 words).\n\nSecond paragraph with more details and analysis (150-200 words).\n\nThird paragraph discussing implications and expert views (150-200 words).\n\nFourth paragraph with additional context and perspectives (150-200 words).\n\nFifth paragraph concluding the main analysis (100-150 words). "",
""Subtitle3"": ""Subtitle for the Conclusion"",
""Conclusion"": ""Your 150-200 word conclusion that summarizes the key points, discusses what this means for the future, and leaves the reader with final thoughts about the significance of this development."",
""Genre"": ""Technology News"",
  ""Image1"": ""https://loremflickr.com/1920/1080/technology,innovation"",
  ""Image2"": ""https://loremflickr.com/1920/1080/future,science""
}

CRITICAL REQUIREMENTS: 
- Write in professional journalistic style with concrete examples
- Use \n\n to separate paragraphs in the Middle section (minimum 4-5 paragraphs)
- Replace example image URLs with keywords matching your specific article topic
- Image1 and Image2 must not be identical
- Respond with valid JSON only - no markdown blocks, no extra text
- Make the content engaging, informative, and well-researched";

                Console.WriteLine("Sending request to OpenAI API...");

                var completion = await client.CompleteChatAsync(prompt);

                var messageContent = completion.Value.Content[0].Text;

                Console.WriteLine("Raw API Response:");
                Console.WriteLine(messageContent);

                var cleanedContent = messageContent.Trim();

                if (cleanedContent.StartsWith("```json"))
                    cleanedContent = cleanedContent.Substring(7);

                if (cleanedContent.StartsWith("```"))
                    cleanedContent = cleanedContent.Substring(3);

                if (cleanedContent.EndsWith("```"))
                    cleanedContent = cleanedContent.Substring(0, cleanedContent.Length - 3);

                cleanedContent = cleanedContent.Trim();

                Console.WriteLine("Cleaned JSON:");
                Console.WriteLine(cleanedContent);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };

                var aiArticle = JsonSerializer.Deserialize<NewspaperArticle>(
                    cleanedContent,
                    options) ?? throw new Exception("Invalid response structure.");

                if (string.IsNullOrEmpty(aiArticle.Title))
                {
                    throw new Exception("Invalid response structure.");
                }

                var image1 = AddUniqueLockParameter(aiArticle.Image1);
                var image2 = EnsureDistinctImageUrl(
                    image1,
                    aiArticle.Image2,
                    "https://loremflickr.com/1920/1080/news,world"
                );

                var article = new NewspaperArticle
                {
                    Title = aiArticle.Title,
                    Subtitle1 = aiArticle.Subtitle1,
                    Introduction = aiArticle.Introduction,
                    Subtitle2 = aiArticle.Subtitle2,
                    MiddleSection = aiArticle.MiddleSection,
                    Subtitle3 = aiArticle.Subtitle3,
                    Conclusion = aiArticle.Conclusion,
                    Genre = aiArticle.Genre,
                    Image1 = image1,
                    Image2 = image2,
                    PublishDate = DateTime.Now,
                    Author = "AI News Bot"
                };

                Console.WriteLine($"Adding article to database: {article.Title}");

                db.Articles.Add(article);

                Console.WriteLine("Saving changes to database...");

                await db.SaveChangesAsync();

                Console.WriteLine("Article successfully saved to database!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("AI nieuws toevoegen mislukt: " + ex.Message);
                Console.WriteLine("Stack trace: " + ex.StackTrace);

                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner exception: " + ex.InnerException.Message);
                }
            }
        }
    }
}
