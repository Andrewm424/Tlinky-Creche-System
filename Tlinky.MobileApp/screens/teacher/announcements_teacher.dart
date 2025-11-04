import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:intl/intl.dart';
import '../../theme/app_theme.dart';
import '../../widgets/tlinky_drawer.dart';
import '../../services/api_service.dart';
import 'dart:convert';
import 'package:http/http.dart' as http;

class TeacherAnnouncementsScreen extends StatefulWidget {
  const TeacherAnnouncementsScreen({super.key});

  @override
  State<TeacherAnnouncementsScreen> createState() =>
      _TeacherAnnouncementsScreenState();
}

class _TeacherAnnouncementsScreenState
    extends State<TeacherAnnouncementsScreen> {
  bool isLoading = true;
  String? error;
  List<dynamic> announcements = [];

  @override
  void initState() {
    super.initState();
    fetchAnnouncements();
  }

  Future<void> fetchAnnouncements() async {
    setState(() {
      isLoading = true;
      error = null;
    });

    try {
      final uri =
          Uri.parse('${ApiService.baseUrl}/AnnouncementsApi?audience=Teachers');
      final res = await http.get(uri);

      if (res.statusCode == 200) {
        setState(() {
          announcements = jsonDecode(res.body);
          isLoading = false;
        });
      } else {
        throw Exception('Server returned ${res.statusCode}');
      }
    } catch (e) {
      setState(() {
        error = e.toString();
        isLoading = false;
      });
    }
  }

  String formatDate(String date) {
    try {
      final dt = DateTime.parse(date);
      return DateFormat('dd MMM yyyy • HH:mm').format(dt);
    } catch (_) {
      return date;
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      drawer: const TlinkyDrawer(),
      backgroundColor: AppTheme.background,
      appBar: AppBar(
        backgroundColor: AppTheme.primary,
        foregroundColor: Colors.white,
        title: const Text('Teacher Announcements'),
        elevation: 0,
      ),
      body: isLoading
          ? const Center(child: CircularProgressIndicator())
          : error != null
              ? Center(
                  child: Text(
                    '⚠️ Failed to load announcements\n$error',
                    textAlign: TextAlign.center,
                    style: GoogleFonts.poppins(color: Colors.red),
                  ),
                )
              : RefreshIndicator(
                  onRefresh: fetchAnnouncements,
                  child: announcements.isEmpty
                      ? ListView(
                          children: [
                            const SizedBox(height: 100),
                            Center(
                              child: Text(
                                'No announcements yet.',
                                style: GoogleFonts.poppins(
                                    fontSize: 16, color: Colors.grey),
                              ),
                            ),
                          ],
                        )
                      : ListView.builder(
                          padding: const EdgeInsets.all(16),
                          itemCount: announcements.length,
                          itemBuilder: (_, i) {
                            final a = announcements[i];
                            final title = a['title'] ?? '';
                            final message = a['message'] ?? '';
                            final audience = a['audience'] ?? 'Everyone';
                            final author = a['author'] ?? '';
                            final date = formatDate(a['datePosted']);

                            return Card(
                              elevation: 3,
                              shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(12),
                              ),
                              margin: const EdgeInsets.only(bottom: 14),
                              child: Padding(
                                padding: const EdgeInsets.all(16),
                                child: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    // Title + Audience badge
                                    Row(
                                      mainAxisAlignment:
                                          MainAxisAlignment.spaceBetween,
                                      children: [
                                        Expanded(
                                          child: Text(
                                            title,
                                            style: GoogleFonts.poppins(
                                              fontSize: 16,
                                              fontWeight: FontWeight.w600,
                                              color: AppTheme.primary,
                                            ),
                                          ),
                                        ),
                                        Container(
                                          padding: const EdgeInsets.symmetric(
                                              horizontal: 10, vertical: 4),
                                          decoration: BoxDecoration(
                                            color: AppTheme.primary
                                                .withOpacity(0.1),
                                            borderRadius:
                                                BorderRadius.circular(12),
                                          ),
                                          child: Text(
                                            audience,
                                            style: GoogleFonts.poppins(
                                              fontSize: 12,
                                              color: AppTheme.primary,
                                              fontWeight: FontWeight.w500,
                                            ),
                                          ),
                                        ),
                                      ],
                                    ),
                                    const SizedBox(height: 8),

                                    // Message body
                                    Text(
                                      message,
                                      style: GoogleFonts.inter(
                                        fontSize: 14,
                                        color: Colors.grey.shade800,
                                      ),
                                    ),
                                    const SizedBox(height: 10),

                                    // Footer: Date + Author
                                    Row(
                                      children: [
                                        const Text('📅 ',
                                            style: TextStyle(fontSize: 12)),
                                        Text(
                                          date,
                                          style: GoogleFonts.inter(
                                              fontSize: 12,
                                              color: Colors.grey.shade600),
                                        ),
                                        if (author.isNotEmpty) ...[
                                          const Text('  •  ✏️ ',
                                              style: TextStyle(fontSize: 12)),
                                          Text(
                                            author,
                                            style: GoogleFonts.inter(
                                                fontSize: 12,
                                                color: Colors.grey.shade600),
                                          ),
                                        ],
                                      ],
                                    ),
                                  ],
                                ),
                              ),
                            );
                          },
                        ),
                ),
    );
  }
}
