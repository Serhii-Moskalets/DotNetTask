import Router from "./router/Router";
import { AuthProvider } from "./providers/AuthProvider";

function App() {
    return (
        <AuthProvider>
            <Router />
        </AuthProvider>
    );
}

export default App;