"use client"; // Make sure to enable client-side functionality

import { useState } from "react";

export default function Home() {
  const [response, setResponse] = useState<string>("");

  const fetchData = async () => {
    try {
      // Make a GET request to the /hello endpoint on your backend
      const res = await fetch("http://localhost:5001/hello");

      if (!res.ok) {
        throw new Error("Network response was not ok");
      }

      const data = await res.json();
      setResponse(data.message || "No message received");
    } catch (error) {
      setResponse("Error fetching data");
    }
  };

  return (
    <div>
      <button
        className="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded"
        onClick={fetchData}
      >
        Call /hello API
      </button>
      <p>{response}</p>
    </div>
  );
}
