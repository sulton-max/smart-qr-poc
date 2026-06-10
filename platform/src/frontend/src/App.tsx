import { CreateCodeScreen } from "./screens/CreateCodeScreen";

export function App() {
  return (
    <div className="min-h-screen bg-background text-foreground">
      <header className="border-b border-border px-6 py-4">
        <span className="text-lg font-bold">Smart QR</span>
        <span className="ml-2 text-sm text-muted-foreground">programmable codes that never expire</span>
      </header>
      <main className="mx-auto max-w-5xl px-6 py-8">
        <CreateCodeScreen />
      </main>
    </div>
  );
}
