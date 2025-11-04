import 'package:flutter/material.dart';
import '../../theme/app_theme.dart';
import '../../widgets/tlinky_parent_drawer.dart';
import '../../services/api_service.dart';

class ParentProfileScreen extends StatefulWidget {
  const ParentProfileScreen({super.key});

  @override
  State<ParentProfileScreen> createState() => _ParentProfileScreenState();
}

class _ParentProfileScreenState extends State<ParentProfileScreen> {
  bool editing = false;

  late final nameCtrl =
      TextEditingController(text: ParentSession.parent?['fullName'] ?? '');
  late final emailCtrl =
      TextEditingController(text: ParentSession.parent?['email'] ?? '');
  late final phoneCtrl =
      TextEditingController(text: ParentSession.parent?['phone'] ?? '');
  late final noteCtrl = TextEditingController(
      text: 'Parent of ${ParentSession.parent?['childName'] ?? 'Unknown Child'}');

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      drawer: const TlinkyParentDrawer(),
      appBar: AppBar(
        backgroundColor: AppTheme.primary,
        foregroundColor: Colors.white,
        title: const Text('My Profile'),
      ),
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
                    backgroundImage: AssetImage('assets/avatars/parent1.png')),
                const SizedBox(height: 16),
                _field('Full Name', nameCtrl),
                _field('Email', emailCtrl),
                _field('Phone', phoneCtrl),
                _field('Notes', noteCtrl),
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
            labelText: label, border: const OutlineInputBorder()),
      ),
    );
  }
}
