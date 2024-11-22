'use client';

import AuthButton from "./components/AuthButton";
import VideoStream from "./components/VideoStream";
import { useAuth } from "./context/AuthContext";

export default function Home() {
  const { isAuthenticated } = useAuth();

  return (
    <main className="flex min-h-screen flex-col items-center p-8">
      <div className="w-full max-w-4xl space-y-8">
        <div className="flex justify-end">
          <AuthButton />
        </div>
        
        {isAuthenticated && (
          <div className="space-y-4">
            <VideoStream />
          </div>
        )}
      </div>
    </main>
  );
}
