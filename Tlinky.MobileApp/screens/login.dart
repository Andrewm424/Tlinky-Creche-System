import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'parent/parent_home.dart'; // ✅ correct import for ParentHome
import 'teacher/teacher_home.dart';
import '../services/api_service.dart'; // ✅ central API service

class LoginPage extends StatefulWidget {
  const LoginPage({super.key});

  @override
  State<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends State<LoginPage> {
  final _usernameController = TextEditingController();
  final _passwordController = TextEditingController();
  String? _error;
  bool _loading = false;

  Future<void> _login() async {
    final user = _usernameController.text.trim();
    final pass = _passwordController.text.trim();

    if (user.isEmpty || pass.isEmpty) {
      setState(() => _error = "Please enter both email and password.");
      return;
    }

    setState(() {
      _error = null;
      _loading = true;
    });

    try {
      // ✅ Step 1: Try Teacher login
      final teacherData = await ApiService.loginTeacher(user, pass);
      if (teacherData != null) {
        TeacherSession.teacher = teacherData;

        if (!mounted) return;
        Navigator.pushReplacement(
          context,
          MaterialPageRoute(builder: (_) => const TeacherHome()),
        );
        return;
      }

      // ✅ Step 2: Try Parent login if not a teacher
      final parentData = await ApiService.loginParent(user, pass);
      if (parentData != null && parentData['success'] == true) {
        ParentSession.parent = parentData;

        if (!mounted) return;
        Navigator.pushReplacement(
          context,
          MaterialPageRoute(builder: (_) => const ParentHome()), // ✅ FIXED HERE
        );
        return;
      }

      // ❌ Step 3: If both fail
      setState(() => _error = "Invalid credentials. Please try again.");
    } catch (e) {
      setState(() => _error = "Connection failed: $e");
    } finally {
      setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Center(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(24),
          child: Column(
            children: [
              const Icon(Icons.school, size: 80, color: Color(0xFF2563EB)),
              const SizedBox(height: 16),
              Text("Tlinky Crèche",
                  style: GoogleFonts.poppins(
                      fontSize: 26, fontWeight: FontWeight.bold)),
              const SizedBox(height: 32),
              Card(
                elevation: 3,
                shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(16)),
                child: Padding(
                  padding: const EdgeInsets.all(20),
                  child: Column(
                    children: [
                      TextField(
                        controller: _usernameController,
                        decoration: const InputDecoration(
                          prefixIcon: Icon(Icons.person),
                          labelText: "Email",
                        ),
                      ),
                      const SizedBox(height: 16),
                      TextField(
                        controller: _passwordController,
                        obscureText: true,
                        decoration: const InputDecoration(
                          prefixIcon: Icon(Icons.lock),
                          labelText: "Password",
                        ),
                      ),
                      if (_error != null) ...[
                        const SizedBox(height: 12),
                        Text(
                          _error!,
                          textAlign: TextAlign.center,
                          style: const TextStyle(color: Colors.red),
                        ),
                      ],
                      const SizedBox(height: 24),
                      _loading
                          ? const CircularProgressIndicator()
                          : FilledButton.icon(
                              icon: const Icon(Icons.login),
                              onPressed: _login,
                              label: const Text("Login"),
                            ),
                    ],
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
