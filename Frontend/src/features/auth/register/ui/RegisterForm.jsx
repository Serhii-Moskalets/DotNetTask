import { Link } from "react-router";
import Button from "../../../../shared/components/Button"
import Input from "../../../../shared/components/Input";
import { ROUTES } from "../../../../shared/constants/routes";
import FormAlert from "../../../../shared/components/FormAlert";

const RegisterForm = ({
    form,
    handleChange,
    handleSubmit,
    error,
    loading,
}) => {
    const showFormError = error && !error.isRateLimit;

    return (
        <div className="w-full max-w-md bg-white rounded-2xl shadow-xl shadow-slate-200 p-8">

            <div className="mb-8 text-center">
                <h1 className="text-2xl font-bold text-slate-800">Create account</h1>
                <p className="text-slate-500 text-sm mt-1">Join us today</p>
            </div>

            <form onSubmit={handleSubmit} className="flex flex-col gap-4">
                <div className="grid grid-cols-2 gap-3">
                    <Input label="First name" type="text" value={form.firstName}
                        onChange={handleChange('firstName')} required placeholder="John" />
                    <Input label="Last name" type="text" value={form.lastName}
                        onChange={handleChange('lastName')} placeholder="Doe" />
                </div>
                <Input label="Username" type="text" value={form.userName}
                    onChange={handleChange('userName')} required placeholder="johndoe" />
                <Input label="Email" type="email" value={form.email}
                    onChange={handleChange('email')} required placeholder="you@example.com" />
                <Input label="Password" type="password" value={form.password}
                    onChange={handleChange('password')} required placeholder="••••••••" />

                {showFormError && <FormAlert error={error} />}

                <Button 
                    type="submit" 
                    loading={loading} 
                    className="mt-2"
                    disabled={error?.isRateLimit}
                >
                    Create account
                </Button>
            </form>

            <p className="text-center text-sm text-slate-500 mt-6">
                Already have an account?{" "}
                <Link to={ROUTES.LOGIN} className="text-indigo-600 font-semibold hover:underline">
                    Sign in
                </Link>
            </p>
        </div>
    );
};

export default RegisterForm;