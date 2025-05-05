# Start ngrok if it's not already running
if (-not (Get-Process ngrok -ErrorAction SilentlyContinue)) {
    ngrok http --url=pet-primate-modern.ngrok-free.app http://localhost:6001
}