# Image Description Generator

## Overview
This repository includes **two C# console applications** for Visual Studio 2022 that generate image descriptions using AI models.

Both projects run on **.NET 8 (Core)** and are compileable for Windows, Linux, and Mac but use different APIs:

- **Img2TxtGPT** — uses the **OpenAI API** (or LM Studio as a local alternative).
- **Img2TxtGemini** — uses the **Google Gemini API**.

## Projects

| Project         | Description                                  |
|-----------------|----------------------------------------------|
| **Img2TxtGPT**  | Uses OpenAI's API (or LM Studio).     |
| **Img2TxtGemini** | Uses Google Gemini API. |

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
<b>Description</b>: "The image depicts a cartoon-style drawing of a frog wearing a red hat. The frog has a green face with black eyes, and its mouth is open as if it's speaking or singing. It is holding a cigar in its mouth, which adds to the whimsical nature of the scene. The frog is dressed in a red suit with a white collar, and there are yellow flowers on its lapel, adding a pop of color to the outfit. The background is a light beige color, providing a neutral backdrop that allows the frog to stand out. The overall style of the drawing is playful and imaginative, with exaggerated features and bright colors."

### LM Studio (IBM Granite)
<p align="center">
  <img src="https://github.com/user-attachments/assets/43e6f75a-3e36-4b16-93a3-4506079b9d27" width="256" height="256"><br/>
  [Flip from Little Nemo comic book]
</p>
<b>Description</b>: "The image shows a humanoid robot with a metallic face sitting in a chair. The robot has a feminine form with detailed anatomical features, including a defined chest, arms, and legs. The robot is connected to multiple wires that extend from various points on its body to the sides of the chair. The robot's head is equipped with a headpiece that has additional wires attached. The background is plain and dark, with a geometric shape resembling a pentagon or star drawn above the robot's head. The image is taken from a frontal angle, capturing the entire body of the robot and the chair it is seated on."

### Gemini gemini-2.0-flash
<p align="center">
  <img src="https://github.com/user-attachments/assets/6a3f4a3d-7864-48f9-afb3-780f27c7e6e8" width="256" height="256"><br/>
  [Painting by Edward Hopper ]
</p>
<b>Description</b>: "The image depicts a small sailboat with several figures aboard, navigating through choppy waters. The sky is a mix of blue and white, suggesting a partly cloudy day. Long lines of clouds stretch across the top of the frame and mirror the pattern of light and dark in the water below.
The sailboat, painted primarily in shades of white, has a single mast with a large white sail that dominates the right side of the composition. Four figures are visible on the boat. One man is standing, holding onto the mast with one hand, wearing white pants and appearing shirtless. Another man with reddish hair is seated further back on the boat, also shirtless. A third figure, closer to the front of the boat, is seated and wearing a white hat, possibly holding an oar. A fourth figure is crouched behind the man with reddish hair.
To the left of the boat, a dark, weathered object protrudes from the water, possibly a buoy or some type of marker. The sea around the boat is rendered with choppy waves and whitecaps, indicating wind and movement. The overall impression is one of a brisk sail on a somewhat rough sea, painted with a sense of light and naturalism."

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
👉 [Download Win-64 Executable](https://github.com/Explo-bot/)

---

## License

This project is open-source under the **MIT License**.
