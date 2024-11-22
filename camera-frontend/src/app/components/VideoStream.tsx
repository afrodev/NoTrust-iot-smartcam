'use client';

import { useEffect, useRef, useState } from 'react';

interface MotionData {
  motionDetected: boolean;
  timestamp: string;
}

export default function VideoStream() {
  const videoRef = useRef<HTMLVideoElement>(null);
  const wsRef = useRef<WebSocket | null>(null);
  const [motionEvents, setMotionEvents] = useState<MotionData[]>([]);
  const [isStreamVisible, setIsStreamVisible] = useState(false);

  useEffect(() => {
    // Connect to WebSocket for motion detection
    const ws = new WebSocket('ws://localhost:5001/api/motion');
    wsRef.current = ws;

    ws.onmessage = (event) => {
      const data: MotionData = JSON.parse(event.data);
      if (data.motionDetected) {
        // Show alert
        alert(`Motion detected at ${new Date(data.timestamp).toLocaleString()}`);
        // Add to motion events list
        setMotionEvents(prev => [data, ...prev].slice(0, 10)); // Keep last 10 events
      }
    };

    return () => {
      if (wsRef.current) {
        wsRef.current.close();
      }
    };
  }, []);

  const toggleStream = () => {
    setIsStreamVisible(!isStreamVisible);
    if (!isStreamVisible && videoRef.current) {
      // In a real implementation, you would set up the video stream here
      // For now, we'll just show a placeholder
      videoRef.current.srcObject = null;
    }
  };

  return (
    <div className="space-y-4">
      <button
        onClick={toggleStream}
        className="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded"
      >
        {isStreamVisible ? 'Hide Stream' : 'Show Stream'}
      </button>

      {isStreamVisible && (
        <div className="relative aspect-video max-w-2xl mx-auto bg-gray-900 rounded-lg overflow-hidden">
          <video
            ref={videoRef}
            className="w-full h-full"
            autoPlay
            playsInline
            muted
          >
            Your browser does not support the video element.
          </video>
          
          {/* Placeholder overlay when no stream */}
          {!videoRef.current?.srcObject && (
            <div className="absolute inset-0 flex items-center justify-center text-white">
              <p>Camera stream placeholder</p>
            </div>
          )}
        </div>
      )}

      {/* Motion Detection Events */}
      <div className="max-w-2xl mx-auto mt-4">
        <h3 className="text-lg font-semibold mb-2">Motion Events</h3>
        <div className="space-y-2">
          {motionEvents.map((event, index) => (
            <p key={index} className="p-2 bg-yellow-100 rounded">
              Motion detected at {new Date(event.timestamp).toLocaleString()}
            </p>
          ))}
        </div>
      </div>
    </div>
  );
}
