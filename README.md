# Image Description Generator

## Overview
This repository includes **two C# console applications** for Visual Studio 2022 that generate image descriptions using AI models.

Both projects run on **.NET 8 (Core)** and are compileable for Windows, Linux, and Mac but use different APIs:

- **Img2TxtGPT** — uses the **OpenAI API** (or LM Studio as a local alternative).
- **Img2TxtGemini** — uses the **Google Gemini API**.

## Projects

| Project         | Description                                  |
|-----------------|----------------------------------------------|
| **Img2TxtGPT**  | Uses OpenAI's GPT models (or LM Studio).     |
| **Img2TxtGemini** | Uses Google Gemini API for vision models. |

---

## Comparison Table

| Feature / Model               | Img2TxtGPT (OpenAI) | Img2TxtGPT (LM Studio) | Img2TxtGemini |
|------------------------------|---------------------|-------------------------|----------------|
| **API Key Required**         | ✅ (OpenAI key)     | ❌                      | ✅ (Gemini key) |
| **Online Access Required**   | ✅                  | ❌ (runs offline)       | ✅              |
| **Base64 Image Input**       | ✅                  | ✅                      | ✅              |
| **Free Tier Available**      | ⚠️ (limited)        | ✅ (fully local)        | ✅              |
| **Supported Formats**        | `.jpg`, `.jpeg`, `.png` | `.jpg`, `.jpeg`, `.png` | `.jpg`, `.jpeg`, `.png` |
| **Output**                   | `.txt` file per image | `.txt` file per image | `.txt` file per image |
| **Prompt Customization**     | ✅                  | ✅                      | ✅              |
| **Example Models**           | `gpt-4o-mini`, `gpt-4o`, `...` | see below | `gemini-2.0-flash`,`...` |

---

## Supported Models

### 🧠 OpenAI (used in Img2TxtGPT)
- `gpt-4o-mini`
- `gpt-4o`
- Requires a paid OpenAI API key.

### 🧠 Gemini (used in Img2TxtGemini)
- `gemini-2.0-flash`
- Free tier available via [Google AI Studio](https://makersuite.google.com/).

### 🧠 Local Vision Models (for LM Studio / Ollama)
You can use these offline models with **Img2TxtGPT** by pointing to a local endpoint like `http://localhost:1234/v1/chat/completions`:

- [IBM Granite 3.2 Vision (2B GGUF)](https://huggingface.co/DevQuasar/ibm-granite.granite-vision-3.2-2b-GGUF)
- [LLaVA v1.5 7B (GGUF)](https://huggingface.co/second-state/Llava-v1.5-7B-GGUF)
- [LLaMA 3.1 Unhinged Vision (8B GGUF)](https://huggingface.co/FiditeNemini/Llama-3.1-Unhinged-Vision-8B-GGUF)

---

## Configuration

### Img2TxtGPT (`App.config`)
```xml
<appSettings>
		<add key="openAIKey" value="YOUR_API_KEY"/>
		<add key="openAIApiEndpoint" value="https://api.openai.com/v1/chat/completions"/>
		<add key="openAIModelName" value="gpt-4o-mini"/>
		<add key="prompt" value="Can you give me a detailed description in English of the image?"/>
</appSettings>
```

👉 For LM Studio: set openAIApiEndpoint to `http://localhost:1234/v1/chat/completions` and fill in the API Key and Template fields with whatever you want.

### Img2TxtGemini (`App.config`)
```xml
<appSettings>
		<add key="geminiKey" value="YOUR_API_KEY"/>
		<add key="geminiModelName" value="gemini-2.0-flash"/>
		<add key="prompt" value="Can you give me a detailed description in English of the image?"/>
</appSettings>
```

---

## Example Outputs

### OpenAI GPT-4o
<p align="center">
  <img src="https://github.com/user-attachments/assets/681b08bd-3bc2-4a84-9376-e86e749db46b" width="256" height="256"><br/>
  [Robot from 'Metropolis' (F. Lang, 1927)]
</p>
**Description**: "The image shows a humanoid robot with a metallic face sitting in a chair..."

### LM Studio (IBM Granite)
<p align="center">
  <img src="https://github.com/user-attachments/assets/43e6f75a-3e36-4b16-93a3-4506079b9d27" width="256" height="256"><br/>
  [Flip from Little Nemo comic book]
</p>
**Description**: "The image depicts a cartoon-style drawing of a frog wearing a red hat..."

---

## How to Use

1. Open the desired project in Visual Studio 2022.
2. Edit the `App.config` file with your API key and endpoint.
3. Build the project.
4. Run the executable with the folder path as argument:
   ```bash
   Img2TxtGPT.exe "C:\path\to\images"
   ```
   or
   ```bash
   Img2TxtGemini.exe "C:\path\to\images"
   ```

5. For each image, a `.txt` file with the description will be generated in the same folder.

---

## Error Handling

- If no folder path is provided, an error message is shown and the app exits.
- If the folder does not exist, an error is shown.
- Image-specific errors are logged to the console.

---

## Windows Executable

You can download two ready-to-use executables for Win-64 with the embedded .NET runtime:  
👉 [Download Win-64 Executable](https://github.com/Explo-bot/GetDescrImg/blob/main/DescrImg.7z)

---

## License

This project is open-source under the **MIT License**.
