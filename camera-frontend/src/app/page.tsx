'use client';

import ApiButton from "./components/ApiButton";
import AuthButton from "./components/AuthButton";
import { useAuth } from "./context/AuthContext";

export default function Home() {
  const { isAuthenticated } = useAuth();

  return (
    <main className="flex items-center justify-center min-h-screen">
      <div className="text-center space-y-4">
        <AuthButton />
        {isAuthenticated && (
          <div className="space-y-4">
            <ApiButton endpoint="/stream">View Camera Stream</ApiButton>
          </div>
        )}
      </div>
    </main>
  );
}
