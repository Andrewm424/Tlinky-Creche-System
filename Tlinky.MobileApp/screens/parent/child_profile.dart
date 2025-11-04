import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../theme/app_theme.dart';
import '../../widgets/tlinky_parent_drawer.dart';
import '../../services/api_service.dart';

class ChildProfile extends StatelessWidget {
  const ChildProfile({super.key});

  @override
  Widget build(BuildContext context) {
    final parent = ParentSession.parent;

    // ✅ Child info fetched from parent login data
    final childName = parent?['childName'] ?? 'Unknown Child';
    final className = parent?['className'] ?? 'Unassigned Class';
    final age = parent?['childAge']?.toString() ?? 'N/A';
    final allergies = parent?['allergies'] ?? 'None';
    final guardian = parent?['fullName'] ?? 'Parent';
    final photoUrl = parent?['childPhoto']; // optional future API value

    return Scaffold(
      drawer: const TlinkyParentDrawer(),
      backgroundColor: AppTheme.background,
      appBar: AppBar(
        backgroundColor: AppTheme.primary,
        foregroundColor: Colors.white,
        title: const Text("Child Profile"),
        elevation: 0,
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(24),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.center,
          children: [
            CircleAvatar(
              radius: 60,
              backgroundImage: photoUrl != null
                  ? NetworkImage(photoUrl)
                  : const AssetImage('assets/avatars/parent1.png')
                      as ImageProvider,
            ),
            const SizedBox(height: 20),
            Text(
              childName,
              style: GoogleFonts.poppins(
                fontSize: 22,
                fontWeight: FontWeight.w700,
              ),
            ),
            const SizedBox(height: 6),
            Text(
              className,
              style: GoogleFonts.inter(color: Colors.grey[700], fontSize: 14),
            ),
            const SizedBox(height: 24),

            // Info Cards
            _infoTile("Age", "$age years", Icons.cake),
            _infoTile("Allergies", allergies, Icons.warning_amber_rounded),
            _infoTile("Guardian", guardian, Icons.family_restroom),
          ],
        ),
      ),
    );
  }

  Widget _infoTile(String label, String value, IconData icon) {
    return Card(
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      margin: const EdgeInsets.symmetric(vertical: 8),
      child: ListTile(
        leading: CircleAvatar(
          backgroundColor: AppTheme.primary.withOpacity(0.1),
          child: Icon(icon, color: AppTheme.primary),
        ),
        title: Text(label,
            style: GoogleFonts.inter(
                color: Colors.grey[600], fontWeight: FontWeight.w500)),
        subtitle: Text(
          value,
          style: GoogleFonts.poppins(fontWeight: FontWeight.w600),
        ),
      ),
    );
  }
}
