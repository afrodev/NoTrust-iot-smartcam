"use client";

import { useState } from "react";

interface ApiButtonProps {
  endpoint: string;
  method?: "GET" | "POST" | "PUT" | "DELETE"; // Add other HTTP methods as needed
  data?: any; // Data to send with the request (for POST, PUT)
  onSuccess?: (data: any) => void; // Callback for successful response
  onError?: (error: Error) => void; // Callback for error
  children: React.ReactNode; // Content of the button
}

const ApiButton: React.FC<ApiButtonProps> = ({
  endpoint,
  method = "GET",
  data: requestData,
  onSuccess,
  onError,
  children,
}) => {
  const [response, setResponse] = useState<string>(""); // Store response data or error message
  const [loading, setLoading] = useState(false); // Manage loading state

  const fetchData = async () => {
    setLoading(true);
    setResponse(""); // Clear previous response on new request

    try {
      const options: RequestInit = {
        method,
        headers: {
          "Content-Type": "application/json",
        },
      };

      if (requestData) {
        options.body = JSON.stringify(requestData); // Use the renamed variable
      }

      const res = await fetch(`http://localhost:5001${endpoint}`, options);

      if (!res.ok) {
        throw new Error("Network response was not ok");
      }

      const data = await res.json();
      setResponse(data.message || "No message received");

      if (onSuccess) {
        onSuccess(data); // Trigger onSuccess callback if provided
      }
    } catch (error: any) {
      setResponse("Error fetching data");
      if (onError) {
        onError(error); // Trigger onError callback if provided
      } else {
        console.error("API request failed:", error);
      }
    } finally {
      setLoading(false); // Reset loading state once request is complete
    }
  };

  return (
    <div>
      <button
        className="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded"
        onClick={fetchData} // Trigger fetchData on button click
      >
        {loading ? "Loading..." : children}
      </button>
      <p>{response}</p> {/* Display response or error message */}
    </div>
  );
};

export default ApiButton;
