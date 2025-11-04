import 'dart:async';
import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../theme/app_theme.dart';
import '../../services/api_service.dart';
import '../../widgets/tlinky_parent_drawer.dart';

// Screens
import 'announcements_parent.dart';
import 'messaging_parent.dart';
import 'profile_parent.dart';
import 'payments_screen.dart';
import 'attendance_history.dart';


class ParentHome extends StatefulWidget {
  const ParentHome({super.key});

  @override
  State<ParentHome> createState() => _ParentHomeState();
}

class _ParentHomeState extends State<ParentHome> {
  bool _showWelcome = true;

  @override
  void initState() {
    super.initState();
    Timer(const Duration(seconds: 2), () {
      if (mounted) setState(() => _showWelcome = false);
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppTheme.background,
      appBar: AppBar(
        backgroundColor: AppTheme.primary,
        foregroundColor: Colors.white,
        title: const Text('Parent Dashboard'),
        elevation: 0,
      ),
      drawer: const TlinkyParentDrawer(),
      body: AnimatedSwitcher(
        duration: const Duration(milliseconds: 700),
        child: _showWelcome ? _buildWelcomeSplash() : _buildDashboard(context),
      ),
    );
  }

  Widget _buildWelcomeSplash() {
    return Center(
      key: const ValueKey('welcome'),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          const Icon(Icons.family_restroom, color: AppTheme.primary, size: 60),
          const SizedBox(height: 16),
          Text(
            "Welcome back, ${ParentSession.name} 👋",
            style: GoogleFonts.poppins(fontSize: 22, fontWeight: FontWeight.w600),
          ),
          const SizedBox(height: 8),
          Text("Here’s what’s new at Tlinky!",
              style: GoogleFonts.inter(color: Colors.grey[600])),
        ],
      ),
    );
  }

  // ✅ UPDATED DASHBOARD SECTION
  Widget _buildDashboard(BuildContext context) {
    return FutureBuilder<Map<String, dynamic>>(
      future: ApiService.getParentOverview(ParentSession.id ?? 0),
      builder: (context, snapshot) {
        if (!snapshot.hasData) {
          return const Center(child: CircularProgressIndicator());
        }
        final data = snapshot.data!;
        return SingleChildScrollView(
          key: const ValueKey('dashboard'),
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              _greetingHeader(),
              const SizedBox(height: 20),
              Text("Your Child’s Summary",
                  style: GoogleFonts.poppins(fontSize: 18, fontWeight: FontWeight.w600)),
              const SizedBox(height: 10),
              GridView.count(
                crossAxisCount: 2,
                shrinkWrap: true,
                physics: const NeverScrollableScrollPhysics(),
                crossAxisSpacing: 12,
                mainAxisSpacing: 12,
                children: [
                  _metricTile(Icons.child_care, "Children", "${data['totalChildren']}", Colors.orange.shade600),
                  _metricTile(Icons.money, "Total Fees", "R${data['totalFees']}", Colors.purple.shade600),
                  _metricTile(Icons.attach_money, "Paid", "R${data['totalPaid']}", Colors.green.shade600),
                  _metricTile(Icons.balance, "Balance", "R${data['balance']}", Colors.red.shade600),
                ],
              ),
              const SizedBox(height: 24),
              Text("Quick Actions",
                  style: GoogleFonts.poppins(fontSize: 18, fontWeight: FontWeight.w600)),
              const SizedBox(height: 12),
              _quickActionButton(context,
                  icon: Icons.payments, title: 'Payments', page: PaymentsScreen()),
              const SizedBox(height: 10),
              _quickActionButton(context,
                  icon: Icons.event_available,
                  title: 'Attendance History',
                  page: AttendanceHistory()),
              const SizedBox(height: 10),
              _quickActionButton(context,
                  icon: Icons.campaign_outlined,
                  title: 'Announcements',
                  page: const ParentAnnouncementsScreen()),
              const SizedBox(height: 10),
              _quickActionButton(context,
                  icon: Icons.chat_bubble_outline,
                  title: 'Messaging',
                  page: const ParentMessagingScreen()),
              const SizedBox(height: 10),
              _quickActionButton(context,
                  icon: Icons.person_outline,
                  title: 'My Profile',
                  page: const ParentProfileScreen()),
            ],
          ),
        );
      },
    );
  }

  Widget _greetingHeader() {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        gradient:
            const LinearGradient(colors: [Color(0xFF2563EB), Color(0xFF1E3A8A)]),
        borderRadius: BorderRadius.circular(18),
        boxShadow: [
          BoxShadow(
              color: Colors.blue.withOpacity(0.25),
              blurRadius: 10,
              offset: const Offset(0, 3))
        ],
      ),
      child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
        Text("Hello 👋",
            style: GoogleFonts.inter(color: Colors.white70, fontSize: 14)),
        Text(ParentSession.name,
            style: GoogleFonts.poppins(
                color: Colors.white, fontSize: 24, fontWeight: FontWeight.w600)),
        const SizedBox(height: 8),
        Text(
            "Parent of ${ParentSession.childName} • ${DateTime.now().toLocal().toString().split(' ')[0]}",
            style: GoogleFonts.inter(color: Colors.white70, fontSize: 13)),
      ]),
    );
  }

  Widget _metricTile(IconData icon, String label, String value, Color color) {
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(color: Colors.black12, blurRadius: 6, offset: const Offset(0, 3))
        ],
      ),
      padding: const EdgeInsets.symmetric(vertical: 16, horizontal: 12),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          CircleAvatar(
            backgroundColor: color.withOpacity(0.1),
            radius: 24,
            child: Icon(icon, color: color, size: 26),
          ),
          const SizedBox(height: 10),
          Text(value,
              textAlign: TextAlign.center,
              style: GoogleFonts.poppins(fontSize: 16, fontWeight: FontWeight.w600)),
          Text(label,
              style: GoogleFonts.inter(fontSize: 13, color: Colors.grey.shade600)),
        ],
      ),
    );
  }

  Widget _quickActionButton(BuildContext context,
      {required IconData icon, required String title, required Widget page}) {
    return InkWell(
      onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => page)),
      borderRadius: BorderRadius.circular(16),
      child: Container(
        padding: const EdgeInsets.symmetric(vertical: 16, horizontal: 20),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(16),
          boxShadow: [
            BoxShadow(
                color: Colors.black.withOpacity(0.05),
                blurRadius: 6,
                offset: const Offset(0, 3))
          ],
        ),
        child: Row(
          children: [
            Icon(icon, color: AppTheme.primary),
            const SizedBox(width: 12),
            Expanded(
              child: Text(title,
                  style:
                      GoogleFonts.poppins(fontSize: 16, fontWeight: FontWeight.w500)),
            ),
            const Icon(Icons.arrow_forward_ios, size: 16, color: Colors.grey),
          ],
        ),
      ),
    );
  }
}
