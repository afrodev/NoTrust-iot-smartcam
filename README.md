# NoTrust IoT Smart Camera

Final exam project for the subject Software Engineering and Testing at Østfold University College

## Project Structure
- `CameraBackend.Api/` - .NET backend server
- `camera-frontend/` - React frontend application
- `ESP32-CameraWebServer.ino` - ESP32 camera firmware
- `CameraBackend.Api.Tests/` - Backend unit tests

## Prerequisites
1. **.NET SDK 8.0** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **Node.js** (v18 or later) - [Download](https://nodejs.org/)
3. **Arduino IDE** (for ESP32 camera) - [Download](https://www.arduino.cc/en/software)
   - Install ESP32 board support in Arduino IDE
   - Required libraries: (list ESP32 specific libraries)

## Setup Instructions

### Backend (.NET)
1. Navigate to the backend directory:
   ```bash
   cd CameraBackend.Api
   ```
2. Restore dependencies:
   ```bash
   dotnet restore
   ```
3. Run the application:
   ```bash
   dotnet run
   ```
The API will be available at `https://localhost:5001`

### Frontend (React)
1. Navigate to the frontend directory:
   ```bash
   cd camera-frontend
   ```
2. Install dependencies:
   ```bash
   npm install
   ```
3. Start the development server:
   ```bash
   npm run dev
   ```
The frontend will be available at `http://localhost:3000`

### ESP32 Camera
1. Open `ESP32-CameraWebServer.ino` in Arduino IDE
2. Select your ESP32 board model which should be the AI Thinker model
3. Upload the code to your ESP32

## Running Tests
### Backend Tests
```bash
cd CameraBackend.Api.Tests
dotnet test
```

### Frontend Tests
```bash
cd camera-frontend
npm test
```

## Environment Variables
Create a `.env` file in the root directory with the following variables:
(List required environment variables)

## Contributing
1. Fork the repository
2. Create your feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License
none