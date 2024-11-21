import ApiButton from "./components/ApiButton";

export default function Home() {
  return (
    <main className="flex items-center justify-center min-h-screen">
      <div className="text-center">
        <ApiButton endpoint="/hello">Call /hello API</ApiButton>
        <ApiButton endpoint="/stream"> See livestream</ApiButton>
      </div>
    </main>
  );
}
