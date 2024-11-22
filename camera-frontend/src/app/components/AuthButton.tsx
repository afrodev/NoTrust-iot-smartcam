'use client';

import { useState } from 'react';
import { useAuth } from '../context/AuthContext';

export default function AuthButton() {
  const { isAuthenticated, signIn, signOut } = useAuth();
  const [isLoading, setIsLoading] = useState(false);

  const handleAuth = async () => {
    setIsLoading(true);
    try {
      if (isAuthenticated) {
        await signOut();
      } else {
        await signIn();
      }
    } catch (error) {
      console.error('Auth error:', error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <button
      onClick={handleAuth}
      disabled={isLoading}
      className={`bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded mb-4 ${
        isLoading ? 'opacity-50 cursor-not-allowed' : ''
      }`}
    >
      {isLoading ? 'Loading...' : isAuthenticated ? 'Sign Out' : 'Sign In'}
    </button>
  );
}
