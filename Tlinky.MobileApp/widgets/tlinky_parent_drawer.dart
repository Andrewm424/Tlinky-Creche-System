import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import '../theme/app_theme.dart';
import '../services/api_service.dart'; // ✅ to access ParentSession

// ✅ Parent Screens
import '../screens/parent/parent_home.dart';
import '../screens/parent/child_profile.dart';
import '../screens/parent/attendance_history.dart';
import '../screens/parent/payments_screen.dart';
// import '../screens/parent/messaging_parent.dart'; // ❌ removed messaging import
import '../screens/parent/announcements_parent.dart';
import '../screens/parent/profile_parent.dart';
import '../screens/login.dart';

class TlinkyParentDrawer extends StatelessWidget {
  const TlinkyParentDrawer({super.key});

  @override
  Widget build(BuildContext context) {
    final parent = ParentSession.parent;
    final fullName = parent?['fullName'] ?? 'Parent';
    final email = parent?['email'] ?? 'Unknown';
    final avatar = 'assets/avatars/parent1.png'; // fallback avatar

    return Drawer(
      child: SafeArea(
        child: Column(
          children: [
            // ---------------- HEADER ----------------
            Container(
              width: double.infinity,
              color: AppTheme.primary.withOpacity(0.9),
              padding: const EdgeInsets.all(20),
              child: Column(
                children: [
                  CircleAvatar(
                    radius: 34,
                    backgroundImage: AssetImage(avatar),
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
                    "Parent Account",
                    style: GoogleFonts.inter(
                      color: Colors.white70,
                      fontSize: 13,
                    ),
                  ),
                ],
              ),
            ),

            // ---------------- MENU ITEMS ----------------
            _drawerItem(
              context,
              icon: Icons.dashboard_outlined,
              label: "Dashboard",
              onTap: () => Navigator.pushReplacement(
                context,
                MaterialPageRoute(builder: (_) => const ParentHome()),
              ),
            ),
            _drawerItem(
              context,
              icon: Icons.child_care,
              label: "Child Profile",
              onTap: () => Navigator.push(
                context,
                MaterialPageRoute(builder: (_) => const ChildProfile()),
              ),
            ),
            _drawerItem(
              context,
              icon: Icons.calendar_month_outlined,
              label: "Attendance History",
              onTap: () => Navigator.push(
                context,
                MaterialPageRoute(builder: (_) => const AttendanceHistory()),
              ),
            ),
            _drawerItem(
              context,
              icon: Icons.payments_outlined,
              label: "Payments",
              onTap: () => Navigator.push(
                context,
                MaterialPageRoute(builder: (_) => const PaymentsScreen()),
              ),
            ),

            // ❌ Messaging temporarily removed for now
            /*
            _drawerItem(
              context,
              icon: Icons.chat_bubble_outline,
              label: "Messaging",
              onTap: () => Navigator.push(
                context,
                MaterialPageRoute(builder: (_) => const ParentMessagingScreen()),
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
                    builder: (_) => const ParentAnnouncementsScreen()),
              ),
            ),

            const Spacer(),
            const Divider(),

            _drawerItem(
              context,
              icon: Icons.person_outline,
              label: "My Profile",
              onTap: () => Navigator.push(
                context,
                MaterialPageRoute(builder: (_) => const ParentProfileScreen()),
              ),
            ),

            // ---------------- LOGOUT ----------------
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
                      'Are you sure you want to log out of your account?',
                    ),
                    actions: [
                      TextButton(
                        onPressed: () => Navigator.pop(context, false),
                        child: const Text('Cancel'),
                      ),
                      ElevatedButton(
                        style: ElevatedButton.styleFrom(
                          backgroundColor: AppTheme.primary,
                          foregroundColor: Colors.white,
                        ),
                        onPressed: () => Navigator.pop(context, true),
                        child: const Text('Logout'),
                      ),
                    ],
                  ),
                );

                if (confirm == true) {
                  ParentSession.parent = null; // ✅ clear session
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

  // 🔹 Reusable menu tile
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
