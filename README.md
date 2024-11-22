# NoTrust IoT Smart Camera

Final exam project for the subject Software Engineering and Testing at Østfold University College

## Project Structure
- `CameraBackend.Api/` - .NET backend server
- `camera-frontend/` - Next.js frontend application
- `ESP32-CameraWebServer.ino` - ESP32 camera firmware
- `CameraBackend.Api.Tests/` - Backend unit tests

## Prerequisites
1. **.NET SDK 8.0** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **Node.js** (v18 or later) - [Download](https://nodejs.org/)
3. **Arduino IDE** (for ESP32 camera) - [Download](https://www.arduino.cc/en/software)
   - Install ESP32 board support in Arduino IDE
   - Required libraries:
     * ESP32 Arduino Core
     * ESP32 Camera Driver
     * WebServer Library for ESP32

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

### Frontend (Next.js)
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
2. Select your ESP32 board model (AI Thinker ESP32-CAM)
3. Configure your WiFi credentials in the code
4. Upload the code to your ESP32

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

To run frontend tests in watch mode:
```bash
npm test -- --watch
```

To check code style and linting:
```bash
npm run lint
```

## Features
- Real-time video streaming from ESP32-CAM
- Motion detection with alerts
- Secure authentication system
- Responsive web interface
- WebSocket communication for real-time updates

## Development Notes

### Frontend Testing
The project uses Jest and React Testing Library for frontend testing. Test files are located in `__tests__` directories next to the components they test.

### Authentication
The application uses a secure authentication system. Make sure the backend is running before testing authentication features.

### WebSocket Communication
The application uses WebSocket for real-time communication:
- Motion detection alerts: `ws://localhost:5001/api/motion`
- Video stream: `ws://localhost:5001/api/stream`

## Environment Variables
Create a `.env.local` file in the frontend directory with:
```env
NEXT_PUBLIC_API_URL=https://localhost:5001
```

## Contributing
1. Fork the repository
2. Create your feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License
none