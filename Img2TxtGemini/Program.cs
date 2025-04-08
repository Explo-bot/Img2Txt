using System;
using System.Configuration; // For App.config
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;      // Requires NuGet package Newtonsoft.Json
using Newtonsoft.Json.Linq;
using System.Linq;          // Added for FirstOrDefault()

class Program
{
    static async Task Main(string[] args)
    {
        // --- Load configuration settings for Gemini ---
        string geminiApiKey = ConfigurationManager.AppSettings["geminiKey"];            // Your Gemini API Key: https://aistudio.google.com/app/u/1/apikey
        string prompt = ConfigurationManager.AppSettings["prompt"];                     // Your prompt for the description
        string geminiModelName = ConfigurationManager.AppSettings["geminiModelName"];   // e.g., "gemini-2.0-flash" : https://ai.google.dev/gemini-api/docs/pricing

        if (string.IsNullOrEmpty(geminiApiKey) || string.IsNullOrEmpty(prompt) || string.IsNullOrEmpty(geminiModelName))
        {
            Console.WriteLine("Please ensure 'geminiKey', 'prompt', and 'geminiModelName' are set in the App.config file.");
            return;
        }

        // Construct the Gemini API URL
        string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{geminiModelName}:generateContent?key={geminiApiKey}";

        // Check if the path argument was provided
        if (args.Length != 1)
        {
            Console.WriteLine("Please provide a folder path or a single image file path as a command-line argument.");
            Console.WriteLine("Example (folder): dotnet run C:\\Path\\To\\Your\\Images");
            Console.WriteLine("Example (file):   dotnet run C:\\Path\\To\\Your\\Images\\my_image.jpg");
            return;
        }

        var inputPath = args[0];
        var client = new HttpClient();

        // --- Check if the input path is a directory or a file ---
        if (Directory.Exists(inputPath))
        {
            Console.WriteLine($"Processing all supported images in folder: {inputPath}");
            // Iterate through all files in the specified folder
            foreach (var filePath in Directory.GetFiles(inputPath))
            {
                // Process the file using the dedicated method
                await ProcessImageAsync(client, filePath, prompt, apiUrl);
                Console.WriteLine("---"); // Separator between files in folder mode
            }
        }
        else if (File.Exists(inputPath))
        {
            Console.WriteLine($"Processing single image file: {inputPath}");
            // Process the single file using the dedicated method
            await ProcessImageAsync(client, inputPath, prompt, apiUrl);
        }
        else
        {
            Console.WriteLine($"Error: The path '{inputPath}' is not a valid file or directory.");
            return;
        }

        Console.WriteLine("Operation completed!");
    }

    /// <summary>
    /// Processes a single image file: sends it to the Gemini API and saves the description.
    /// </summary>
    /// <param name="client">The HttpClient instance.</param>
    /// <param name="filePath">The full path to the image file.</param>
    /// <param name="prompt">The text prompt to use with the image.</param>
    /// <param name="apiUrl">The fully constructed Gemini API URL (with model and key).</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    static async Task ProcessImageAsync(HttpClient client, string filePath, string prompt, string apiUrl)
    {
        string fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
        string mimeType = null;

        // Determine the correct MIME type based on the file extension
        if (fileExtension == ".jpg" || fileExtension == ".jpeg")
        {
            mimeType = "image/jpeg";
        }
        else if (fileExtension == ".png")
        {
            mimeType = "image/png";
        }
        // Add more supported types here if needed (e.g., webp, heic, heif, gif)

        // If the file is not a supported image type, skip it
        if (mimeType == null)
        {
            // Only log skipping message if it's not the primary file being processed
            // (Avoids noise if user explicitly passes an unsupported file)
            // We can refine this logging if needed. For now, just return silently.
            // Console.WriteLine($"Skipping unsupported file type: {Path.GetFileName(filePath)}");
            return;
        }

        Console.WriteLine($"Processing: {Path.GetFileName(filePath)}"); // Log which file is being processed now
        try
        {
            // Read image file bytes and convert to Base64
            var imageBytes = File.ReadAllBytes(filePath);
            var base64Image = Convert.ToBase64String(imageBytes);

            // --- Prepare the request payload for the Gemini API ---
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new { text = prompt }, // Text prompt part
                            new                    // Image data part
                            {
                                inlineData = new
                                {
                                    mimeType = mimeType,
                                    data = base64Image
                                }
                            }
                        }
                    }
                }
                // Optional: Add 'generationConfig' here if needed
            };

            // Serialize the request body to JSON
            var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

            // Send the POST request to the Gemini API
            var response = await client.PostAsync(apiUrl, content);
            var responseString = await response.Content.ReadAsStringAsync();

            // Check the HTTP status code for errors
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error calling Gemini API for {filePath}: {(int)response.StatusCode} {response.ReasonPhrase}");
                Console.WriteLine($"Response: {responseString}");
                return; // Stop processing this file on HTTP error
            }

            // Parse the JSON response from Gemini
            var responseObject = JObject.Parse(responseString);

            // Check for API errors within the JSON response structure
            if (responseObject["error"] != null)
            {
                Console.WriteLine($"Gemini API Error for {filePath}: {responseObject["error"]["message"]}");
                return; // Stop processing this file on API error
            }

            // Safely extract the generated text description
            var description = responseObject["candidates"]?.FirstOrDefault()?["content"]?["parts"]?.FirstOrDefault()?["text"]?.ToString();

            if (string.IsNullOrEmpty(description))
            {
                Console.WriteLine($"Could not extract description from Gemini response for {filePath}. It might be empty or blocked.");
                Console.WriteLine($"Full Response: {responseString}");
                description = "Description not generated or found."; // Use placeholder text
            }

            // --- Save the description ---
            // Determine output directory (same as input file's directory)
            var outputDirectory = Path.GetDirectoryName(filePath);
            // Generate the output text file path
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            var outputFilePath = Path.Combine(outputDirectory ?? string.Empty, $"{fileNameWithoutExtension}.txt"); // Handle null outputDirectory just in case

            if (string.IsNullOrEmpty(outputDirectory))
            {
                Console.WriteLine($"Error: Could not determine output directory for {filePath}. Saving in current directory.");
                outputFilePath = $"{fileNameWithoutExtension}.txt"; // Fallback to current directory
            }


            // Save the description to the text file
            File.WriteAllText(outputFilePath, description.Trim()); // Trim whitespace

            Console.WriteLine($"Description saved to: {outputFilePath}");
        }
        catch (IOException ioEx)
        {
            Console.WriteLine($"File Error processing {filePath}: {ioEx.Message}");
        }
        catch (HttpRequestException httpEx)
        {
            Console.WriteLine($"Network Error processing {filePath}: {httpEx.Message}");
        }
        catch (JsonException jsonEx)
        {
            Console.WriteLine($"Error parsing JSON response for {filePath}: {jsonEx.Message}");
        }
        catch (Exception ex)
        {
            // Catch potential errors during file processing or API call
            Console.WriteLine($"An unexpected error occurred while processing file {filePath}: {ex.Message}");
            // For debugging, log the full exception: Console.WriteLine(ex.ToString());
        }
    }
}