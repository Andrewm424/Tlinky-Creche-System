import 'dart:async';
import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:http/http.dart' as http;
import '../../theme/app_theme.dart';
import '../../widgets/tlinky_drawer.dart';
import '../../services/api_service.dart';
import 'attendance.dart' as attendance;
import 'incidents.dart';
import 'announcements_teacher.dart';

class TeacherHome extends StatefulWidget {
  const TeacherHome({super.key});

  @override
  State<TeacherHome> createState() => _TeacherHomeState();
}

class _TeacherHomeState extends State<TeacherHome> {
  bool _showWelcome = true;
  bool _loadingDashboard = true;

  Map<String, dynamic> dashboardData = {
    'totalChildren': 0,
    'presentCount': 0,
    'incidentsCount': 0,
    'announcementsCount': 0,
  };

  @override
  void initState() {
    super.initState();

    // Small welcome splash delay
    Timer(const Duration(seconds: 2), () {
      if (mounted) setState(() => _showWelcome = false);
    });

    _loadDashboardData();
  }

  Future<void> _loadDashboardData() async {
    try {
      final classId = TeacherSession.teacher?['classId'] ?? 0;
      final teacherId = TeacherSession.teacher?['teacherId'] ?? 0;

      // ✅ Corrected endpoint names (no "s" in IncidentApi)
      final summaryUrl =
          Uri.parse('${ApiService.baseUrl}/AttendanceApi/summary?classId=$classId');
      final incidentsUrl =
          Uri.parse('${ApiService.baseUrl}/IncidentApi/count?teacherId=$teacherId');
      final announcementsUrl =
          Uri.parse('${ApiService.baseUrl}/AnnouncementsApi/count?audience=Teachers');

      final responses = await Future.wait([
        http.get(summaryUrl),
        http.get(incidentsUrl),
        http.get(announcementsUrl),
      ]);

      final totalChildren = jsonDecode(responses[0].body)['totalChildren'] ?? 0;
      final presentCount = jsonDecode(responses[0].body)['presentCount'] ?? 0;
      final incidentsCount = jsonDecode(responses[1].body)['count'] ?? 0;
      final announcementsCount = jsonDecode(responses[2].body)['count'] ?? 0;

      if (mounted) {
        setState(() {
          dashboardData = {
            'totalChildren': totalChildren,
            'presentCount': presentCount,
            'incidentsCount': incidentsCount,
            'announcementsCount': announcementsCount,
          };
          _loadingDashboard = false;
        });
      }
    } catch (e) {
      debugPrint("❌ Dashboard load failed: $e");
      if (mounted) setState(() => _loadingDashboard = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final teacherName = TeacherSession.name;
    final className = TeacherSession.className;

    return Scaffold(
      drawer: const TlinkyDrawer(),
      backgroundColor: AppTheme.background,
      appBar: AppBar(
        backgroundColor: AppTheme.primary,
        foregroundColor: Colors.white,
        elevation: 0,
        title: const Text('Teacher Dashboard'),
      ),
      body: AnimatedSwitcher(
        duration: const Duration(milliseconds: 600),
        child: _showWelcome
            ? _buildWelcomeSplash(teacherName)
            : _loadingDashboard
                ? const Center(child: CircularProgressIndicator())
                : _buildDashboard(context, teacherName, className),
      ),
    );
  }

  Widget _buildWelcomeSplash(String name) {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          const Icon(Icons.school, color: AppTheme.primary, size: 60),
          const SizedBox(height: 16),
          Text("Welcome back, $name 👋",
              style: GoogleFonts.poppins(
                  fontSize: 22, fontWeight: FontWeight.w600)),
          const SizedBox(height: 8),
          Text("Loading your class dashboard...",
              style: GoogleFonts.inter(color: Colors.grey[600])),
        ],
      ),
    );
  }

  Widget _buildDashboard(BuildContext context, String name, String className) {
    return RefreshIndicator(
      onRefresh: _loadDashboardData,
      child: SingleChildScrollView(
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            _headerCard(name, className),
            const SizedBox(height: 20),
            Text("Today's Overview",
                style: GoogleFonts.poppins(
                    fontSize: 18, fontWeight: FontWeight.w600)),
            const SizedBox(height: 10),
            GridView.count(
              crossAxisCount: 2,
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              crossAxisSpacing: 12,
              mainAxisSpacing: 12,
              children: [
                _metricTile(context, Icons.people_alt_rounded, "Students",
                    "${dashboardData['totalChildren']} Total", Colors.orange, () {
                  _showModalInfo(context, "Students in $className",
                      "Total students: ${dashboardData['totalChildren']}");
                }),
                _metricTile(context, Icons.check_circle_rounded, "Present",
                    "${dashboardData['presentCount']} Today", Colors.green, () {
                  Navigator.push(
                    context,
                    MaterialPageRoute(
                        builder: (_) => const attendance.AttendanceScreen()),
                  );
                }),
                _metricTile(context, Icons.report_problem_rounded, "Incidents",
                    "${dashboardData['incidentsCount']} Logged", Colors.red, () {
                  Navigator.push(
                    context,
                    MaterialPageRoute(builder: (_) => const IncidentsScreen()),
                  );
                }),
                _metricTile(
                  context,
                  Icons.campaign_rounded,
                  "Announcements",
                  "${dashboardData['announcementsCount']} New",
                  Colors.blue,
                  () {
                    Navigator.push(
                      context,
                      MaterialPageRoute(
                          builder: (_) => const TeacherAnnouncementsScreen()),
                    );
                  },
                ),
              ],
            ),
            const SizedBox(height: 24),
            Center(
              child: FilledButton.icon(
                onPressed: () => Navigator.push(
                  context,
                  MaterialPageRoute(
                      builder: (_) => const attendance.AttendanceScreen()),
                ),
                icon: const Icon(Icons.how_to_reg, color: Colors.white),
                label: Text("Mark Attendance",
                    style: GoogleFonts.poppins(
                        color: Colors.white, fontWeight: FontWeight.w500)),
                style: FilledButton.styleFrom(
                  backgroundColor: AppTheme.primary,
                  padding:
                      const EdgeInsets.symmetric(vertical: 14, horizontal: 20),
                  shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(14)),
                ),
              ),
            ),
            const SizedBox(height: 20),
          ],
        ),
      ),
    );
  }

  Widget _headerCard(String name, String className) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        gradient: const LinearGradient(
          colors: [Color(0xFF2563EB), Color(0xFF1E3A8A)],
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
        ),
        borderRadius: BorderRadius.circular(18),
        boxShadow: [
          BoxShadow(
            color: Colors.blue.withOpacity(0.25),
            blurRadius: 10,
            offset: const Offset(0, 3),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text("Good morning ☀️",
              style: GoogleFonts.inter(color: Colors.white70, fontSize: 14)),
          Text(name,
              style: GoogleFonts.poppins(
                  color: Colors.white,
                  fontSize: 24,
                  fontWeight: FontWeight.w600)),
          const SizedBox(height: 8),
          Text("$className • ${DateTime.now().toLocal().toString().split(' ')[0]}",
              style: GoogleFonts.inter(color: Colors.white70, fontSize: 13)),
        ],
      ),
    );
  }

  Widget _metricTile(BuildContext context, IconData icon, String label,
      String value, Color color, VoidCallback onTap) {
    return InkWell(
      borderRadius: BorderRadius.circular(16),
      onTap: onTap,
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 250),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(16),
          boxShadow: [
            BoxShadow(
              color: Colors.black12,
              blurRadius: 6,
              offset: const Offset(0, 3),
            ),
          ],
        ),
        padding: const EdgeInsets.symmetric(vertical: 18, horizontal: 12),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            CircleAvatar(
              backgroundColor: color.withOpacity(0.1),
              radius: 24,
              child: Icon(icon, color: color, size: 26),
            ),
            const SizedBox(height: 12),
            Text(value,
                style: GoogleFonts.poppins(
                    fontSize: 18, fontWeight: FontWeight.w600)),
            Text(label,
                style: GoogleFonts.inter(
                    fontSize: 13, color: Colors.grey.shade600)),
          ],
        ),
      ),
    );
  }

  void _showModalInfo(BuildContext context, String title, String message) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        title: Text(title,
            style: GoogleFonts.poppins(fontWeight: FontWeight.w600)),
        content: Text(message, style: GoogleFonts.inter(fontSize: 14)),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text("Close"),
          ),
        ],
      ),
    );
  }
}
