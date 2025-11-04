import 'package:flutter/material.dart';
import '../../theme/app_theme.dart';
import '../../widgets/tlinky_drawer.dart';
import '../../services/api_service.dart';

class TeacherProfileScreen extends StatefulWidget {
  const TeacherProfileScreen({super.key});
  @override
  State<TeacherProfileScreen> createState() => _TeacherProfileScreenState();
}

class _TeacherProfileScreenState extends State<TeacherProfileScreen> {
  bool editing = false;

  late final nameCtrl =
      TextEditingController(text: TeacherSession.teacher?['fullName'] ?? '');
  late final emailCtrl =
      TextEditingController(text: TeacherSession.teacher?['email'] ?? '');
  late final classCtrl =
      TextEditingController(text: TeacherSession.teacher?['className'] ?? '');

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      drawer: const TlinkyDrawer(),
      appBar: AppBar(
          backgroundColor: AppTheme.primary,
          foregroundColor: Colors.white,
          title: const Text('My Profile')),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: Card(
          shape:
              RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
          child: Padding(
            padding: const EdgeInsets.all(20),
            child: Column(
              children: [
                const CircleAvatar(
                    radius: 40,
                    backgroundImage: AssetImage('assets/avatars/teacher.png')),
                const SizedBox(height: 16),
                _field('Full Name', nameCtrl),
                _field('Email', emailCtrl),
                _field('Class', classCtrl),
                const SizedBox(height: 20),
                FilledButton.icon(
                  onPressed: () {
                    setState(() => editing = !editing);
                    if (!editing) {
                      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(
                          content: Text('Profile updated successfully.')));
                    }
                  },
                  icon: Icon(editing ? Icons.save : Icons.edit),
                  label: Text(editing ? 'Save' : 'Edit Profile'),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _field(String label, TextEditingController c) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8),
      child: TextField(
        controller: c,
        enabled: editing,
        decoration: InputDecoration(
          labelText: label,
          border: const OutlineInputBorder(),
        ),
      ),
    );
  }
}
