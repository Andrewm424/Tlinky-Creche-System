import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import '../theme/app_theme.dart';
import '../screens/teacher/teacher_home.dart';
import '../screens/teacher/attendance.dart' as attendance;
import '../screens/teacher/incidents.dart';
// import '../screens/teacher/messaging_teacher.dart'; // ❌ removed messaging import
import '../screens/teacher/profile_teacher.dart';
import '../screens/teacher/announcements_teacher.dart';
import '../screens/login.dart';
import '../services/api_service.dart'; // ✅ import session

class TlinkyDrawer extends StatelessWidget {
  const TlinkyDrawer({super.key});

  @override
  Widget build(BuildContext context) {
    final teacher = TeacherSession.teacher;
    final fullName = teacher?['fullName'] ?? 'Unknown';
    final className = teacher?['className'] ?? 'Unassigned';

    return Drawer(
      child: SafeArea(
        child: Column(
          children: [
            // Header section
            Container(
              width: double.infinity,
              color: AppTheme.primary.withOpacity(0.9),
              padding: const EdgeInsets.all(20),
              child: Column(
                children: [
                  CircleAvatar(
                    radius: 34,
                    backgroundImage:
                        const AssetImage('assets/avatars/teacher.png'),
                    backgroundColor: Colors.white.withOpacity(0.6),
                  ),
                  const SizedBox(height: 10),
                  Text(
                    fullName,
                    style: GoogleFonts.poppins(
                      color: Colors.white,
                      fontSize: 18,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                  Text(
                    className,
                    style: GoogleFonts.inter(
                      color: Colors.white70,
                      fontSize: 13,
                    ),
                  ),
                ],
              ),
            ),

            // Navigation items
            _drawerItem(
              context,
              icon: Icons.dashboard_outlined,
              label: "Dashboard",
              onTap: () => Navigator.pushReplacement(
                context,
                MaterialPageRoute(builder: (_) => const TeacherHome()),
              ),
            ),
            _drawerItem(
              context,
              icon: Icons.how_to_reg_outlined,
              label: "Attendance",
              onTap: () => Navigator.push(
                context,
                MaterialPageRoute(
                    builder: (_) => const attendance.AttendanceScreen()),
              ),
            ),
            _drawerItem(
              context,
              icon: Icons.report_gmailerrorred_outlined,
              label: "Incidents",
              onTap: () => Navigator.push(
                context,
                MaterialPageRoute(builder: (_) => const IncidentsScreen()),
              ),
            ),

            // ❌ Messaging removed for now (can re-enable later)
            /*
            _drawerItem(
              context,
              icon: Icons.chat_bubble_outline,
              label: "Messaging",
              onTap: () => Navigator.push(
                context,
                MaterialPageRoute(
                    builder: (_) => const TeacherMessagingScreen()),
              ),
            ),
            */

            _drawerItem(
              context,
              icon: Icons.campaign_outlined,
              label: "Announcements",
              onTap: () => Navigator.push(
                context,
                MaterialPageRoute(
                    builder: (_) => const TeacherAnnouncementsScreen()),
              ),
            ),

            const Spacer(),
            const Divider(),

            _drawerItem(
              context,
              icon: Icons.person_outline,
              label: "Profile",
              onTap: () => Navigator.push(
                context,
                MaterialPageRoute(
                    builder: (_) => const TeacherProfileScreen()),
              ),
            ),

            // ✅ Logout confirmation dialog
            ListTile(
              leading: const Icon(Icons.logout_outlined),
              title: const Text('Logout'),
              onTap: () async {
                final confirm = await showDialog<bool>(
                  context: context,
                  builder: (context) => AlertDialog(
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(16),
                    ),
                    title: const Text('Confirm Logout'),
                    content: const Text(
                        'Are you sure you want to log out of your account?'),
                    actions: [
                      TextButton(
                        onPressed: () => Navigator.pop(context, false),
                        child: const Text('Cancel'),
                      ),
                      ElevatedButton(
                        style: ElevatedButton.styleFrom(
                          backgroundColor: AppTheme.primary,
                          foregroundColor: Colors.white,
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(8),
                          ),
                        ),
                        onPressed: () => Navigator.pop(context, true),
                        child: const Text('Logout'),
                      ),
                    ],
                  ),
                );

                if (confirm == true) {
                  // ✅ clear session on logout
                  TeacherSession.teacher = null;

                  Navigator.of(context).pushAndRemoveUntil(
                    MaterialPageRoute(builder: (_) => const LoginPage()),
                    (route) => false,
                  );

                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(
                      content: Text('You have been logged out.'),
                      behavior: SnackBarBehavior.floating,
                    ),
                  );
                }
              },
            ),
            const SizedBox(height: 10),
          ],
        ),
      ),
    );
  }

  // 🔹 Reusable drawer item widget
  Widget _drawerItem(
    BuildContext context, {
    required IconData icon,
    required String label,
    required VoidCallback onTap,
  }) {
    return ListTile(
      leading: Icon(icon),
      title: Text(label),
      onTap: () {
        Navigator.pop(context);
        onTap();
      },
    );
  }
}
