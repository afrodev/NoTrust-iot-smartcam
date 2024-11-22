"use client";

import { useState } from "react";

/**
 * Props interface for the ApiButton component
 * Defines the structure of data that can be passed to the button
 */
interface ApiButtonProps {
  endpoint: string;
  method?: "GET" | "POST" | "PUT" | "DELETE";  // HTTP method for the API request
  data?: Record<string, unknown>;  // Type-safe data object to send with the request (for POST, PUT)
  onSuccess?: (data: Record<string, unknown>) => void;  // Callback function when API call succeeds
  onError?: (error: Error) => void;  // Callback function when API call fails
  children: React.ReactNode;  // Content to display inside the button
}

/**
 * A reusable button component that makes API calls
 * Handles loading states, error handling, and success callbacks
 */
const ApiButton: React.FC<ApiButtonProps> = ({
  endpoint,
  method = "GET",
  data: requestData,
  onSuccess,
  onError,
  children,
}) => {
  const [response, setResponse] = useState<string>("");  // Stores API response message
  const [loading, setLoading] = useState(false);  // Tracks loading state during API calls

  /**
   * Handles the API request when button is clicked
   * Manages loading states and error handling
   */
  const fetchData = async () => {
    setLoading(true);
    setResponse("");  // Clear previous response

    try {
      // Configure request options
      const options: RequestInit = {
        method,
        headers: {
          "Content-Type": "application/json",
        },
      };

      // Add request body if data is provided
      if (requestData) {
        options.body = JSON.stringify(requestData);
      }

      // Make the API call
      const res = await fetch(`http://localhost:5001${endpoint}`, options);

      if (!res.ok) {
        throw new Error(`HTTP error! status: ${res.status}`);
      }

      // Handle successful response
      const data = await res.json();
      setResponse(data.message || "No message received");

      // Trigger success callback if provided
      if (onSuccess) {
        onSuccess(data);
      }
    } catch (error: unknown) {
      // Handle error cases
      setResponse("Error fetching data");
      if (onError && error instanceof Error) {
        onError(error);
      } else {
        console.error("API request failed:", error);
      }
    } finally {
      setLoading(false);  // Reset loading state
    }
  };

  return (
    <div>
      <button
        className="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded"
        onClick={fetchData}
        disabled={loading}
      >
        {loading ? "Loading..." : children}
      </button>
      <p>{response}</p>  {/* Display API response or error message */}
    </div>
  );
};

export default ApiButton;
