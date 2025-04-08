using System.Configuration;         // For App.config
using System.Net.Http.Headers;      // Added for Authorization header
using System.Text;
using Newtonsoft.Json;              // Requires NuGet package Newtonsoft.Json
using Newtonsoft.Json.Linq;

class Program
{
    static async Task Main(string[] args)
    {
        // --- Load configuration settings for OpenAI ---
        string openAIKey = ConfigurationManager.AppSettings["openAIKey"]; // https://platform.openai.com/account/api-keys.
        string prompt = ConfigurationManager.AppSettings["prompt"];
        string openAIModelName = ConfigurationManager.AppSettings["openAIModelName"]; // e.g., "gpt-4o", "gpt-4-vision-preview"
        string openAIApiEndpoint = ConfigurationManager.AppSettings["openAIApiEndpoint"]; // OpenAI endpoint

        if (string.IsNullOrEmpty(openAIKey) || string.IsNullOrEmpty(prompt) || string.IsNullOrEmpty(openAIModelName))
        {
            Console.WriteLine("Please ensure 'openAIKey', 'prompt', and 'openAIModelName' are set in the App.config file.");
            return;
        }

        // Check if the path argument was provided
        if (args.Length != 1)
        {
            Console.WriteLine("Please provide a folder path or a single image file path as a command-line argument.");
            Console.WriteLine("Example (folder): dotnet run C:\\Path\\To\\Your\\Images");
            Console.WriteLine("Example (file):   dotnet run C:\\Path\\To\\Your\\Images\\my_image.jpg");
            return;
        }

        var inputPath = args[0];

        // --- Setup HttpClient for OpenAI ---
        // Create a single HttpClient instance for efficiency
        var client = new HttpClient();
        // Set the authorization header required by OpenAI
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAIKey);

        // --- Check if the input path is a directory or a file ---
        if (Directory.Exists(inputPath))
        {
            Console.WriteLine($"Processing all supported images in folder: {inputPath}");
            // Iterate through all files in the specified folder
            foreach (var filePath in Directory.GetFiles(inputPath))
            {
                // Process the file using the dedicated method
                await ProcessImageAsync(client, filePath, prompt, openAIModelName, openAIApiEndpoint);
                Console.WriteLine("---"); // Separator between files in folder mode
            }
        }
        else if (File.Exists(inputPath))
        {
            Console.WriteLine($"Processing single image file: {inputPath}");
            // Process the single file using the dedicated method
            await ProcessImageAsync(client, inputPath, prompt, openAIModelName, openAIApiEndpoint);
        }
        else
        {
            Console.WriteLine($"Error: The path '{inputPath}' is not a valid file or directory.");
            return;
        }

        Console.WriteLine("Operation completed!");
    }

    /// <summary>
    /// Processes a single image file: sends it to the OpenAI API and saves the description.
    /// </summary>
    /// <param name="client">The HttpClient instance (with auth header set).</param>
    /// <param name="filePath">The full path to the image file.</param>
    /// <param name="prompt">The text prompt to use with the image.</param>
    /// <param name="modelName">The OpenAI model name (e.g., "gpt-4o").</param>
    /// <param name="apiEndpoint">The OpenAI API endpoint (e.g., "gpt-4o").</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    static async Task ProcessImageAsync(HttpClient client, string filePath, string prompt, string modelName, string apiEndpoint)
    {
        string fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
        string mimeType = null;

        // Determine the correct MIME type based on the file extension
        // OpenAI Vision supports JPEG, PNG, GIF, WEBP
        if (fileExtension == ".jpg" || fileExtension == ".jpeg")
        {
            mimeType = "image/jpeg";
        }
        else if (fileExtension == ".png")
        {
            mimeType = "image/png";
        }
        else if (fileExtension == ".gif")
        {
            mimeType = "image/gif";
        }
        else if (fileExtension == ".webp")
        {
            mimeType = "image/webp";
        }
        // Add more supported types if needed and if supported by the model

        // If the file is not a supported image type, skip it
        if (mimeType == null)
        {
            // Console.WriteLine($"Skipping unsupported file type: {Path.GetFileName(filePath)}");
            return;
        }

        Console.WriteLine($"Processing: {Path.GetFileName(filePath)}"); // Log which file is being processed now
        try
        {
            // Read image file bytes and convert to Base64
            var imageBytes = await File.ReadAllBytesAsync(filePath); // Use async read
            var base64Image = Convert.ToBase64String(imageBytes);

            // Construct the data URI for the image
            string dataUri = $"data:{mimeType};base64,{base64Image}";

            // --- Prepare the request payload for the OpenAI API ---
            var requestBody = new
            {
                model = modelName,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[] // Content can be an array of text and image parts
                        {
                            new { type = "text", text = prompt },
                            new {
                                type = "image_url",
                                image_url = new { url = dataUri }
                            }
                        }
                    }
                },
                max_tokens = 500 // Optional: Limit the length of the response
            };

            // Serialize the request body to JSON
            var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

            // Send the POST request to the OpenAI API
            // Endpoint is constant, auth header is already set on client
            var response = await client.PostAsync(apiEndpoint, content);
            var responseString = await response.Content.ReadAsStringAsync();

            // Check the HTTP status code for errors
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error calling OpenAI API for {filePath}: {(int)response.StatusCode} {response.ReasonPhrase}");
                Console.WriteLine($"Response: {responseString}");
                // Try parsing error details from OpenAI response
                try
                {
                    var errorObj = JObject.Parse(responseString);
                    Console.WriteLine($"OpenAI Error Message: {errorObj?["error"]?["message"]?.ToString()}");
                }
                catch { /* Ignore parsing error if response is not JSON */ }
                return; // Stop processing this file on HTTP error
            }

            // Parse the JSON response from OpenAI
            var responseObject = JObject.Parse(responseString);

            // Check for API errors within the JSON response structure (redundant if status code was checked, but good practice)
            if (responseObject["error"] != null)
            {
                Console.WriteLine($"OpenAI API Error for {filePath}: {responseObject["error"]["message"]}");
                return; // Stop processing this file on API error
            }

            // Safely extract the generated text description
            // Path: choices -> [0] -> message -> content
            var description = responseObject["choices"]?.FirstOrDefault()?["message"]?["content"]?.ToString();

            if (string.IsNullOrEmpty(description))
            {
                Console.WriteLine($"Could not extract description from OpenAI response for {filePath}. It might be empty.");
                Console.WriteLine($"Full Response: {responseString}");
                description = "Description not generated or found."; // Use placeholder text
            }

            // --- Save the description ---
            var outputDirectory = Path.GetDirectoryName(filePath);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            var outputFilePath = Path.Combine(outputDirectory ?? string.Empty, $"{fileNameWithoutExtension}.txt");

            if (string.IsNullOrEmpty(outputDirectory))
            {
                Console.WriteLine($"Error: Could not determine output directory for {filePath}. Saving in current directory.");
                outputFilePath = $"{fileNameWithoutExtension}.txt"; // Fallback to current directory
            }

            // Save the description to the text file (use async write)
            await File.WriteAllTextAsync(outputFilePath, description.Trim()); // Trim whitespace

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