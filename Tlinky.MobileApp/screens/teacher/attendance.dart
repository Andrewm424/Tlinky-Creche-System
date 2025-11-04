import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:intl/intl.dart';
import '../../theme/app_theme.dart';
import '../../widgets/tlinky_drawer.dart';
import '../../services/api_service.dart';

class AttendanceScreen extends StatefulWidget {
  const AttendanceScreen({super.key});

  @override
  State<AttendanceScreen> createState() => _AttendanceScreenState();
}

class _AttendanceScreenState extends State<AttendanceScreen> {
  List<dynamic> records = [];
  bool isLoading = true;
  String? errorMessage;

  @override
  void initState() {
    super.initState();
    fetchAttendance();
  }

  Future<void> fetchAttendance() async {
    try {
      // ✅ Use the logged-in teacher’s class ID
      final classId = TeacherSession.teacher?['classId'] ?? 1;
      final data = await ApiService.getAttendance(classId: classId);
      setState(() {
        records = data;
        isLoading = false;
      });
    } catch (e) {
      setState(() {
        errorMessage = e.toString();
        isLoading = false;
      });
    }
  }

  Future<void> saveAttendance() async {
    try {
      for (var record in records) {
        // ✅ Attach teacherId and clean up null attendanceId
        record['teacherId'] = TeacherSession.teacher?['teacherId'];

        if (record['attendanceId'] == null ||
            record['attendanceId'].toString().isEmpty) {
          record.remove('attendanceId');
        }

        await ApiService.saveAttendance(record);
      }

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text("✅ Attendance saved successfully!")),
        );
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('❌ Save failed: $e')),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final formattedDate =
        DateFormat('EEEE, dd MMM yyyy').format(DateTime.now());
    final className = TeacherSession.className;
    final teacherName = TeacherSession.name;

    return Scaffold(
      drawer: const TlinkyDrawer(),
      backgroundColor: AppTheme.background,
      appBar: AppBar(
        backgroundColor: AppTheme.primary,
        foregroundColor: Colors.white,
        title: Text("Mark Attendance - $className"),
        elevation: 0,
      ),
      body: isLoading
          ? const Center(child: CircularProgressIndicator())
          : errorMessage != null
              ? Center(
                  child: Text(
                    "Error loading data:\n$errorMessage",
                    textAlign: TextAlign.center,
                    style: GoogleFonts.inter(color: Colors.red),
                  ),
                )
              : Padding(
                  padding: const EdgeInsets.all(16),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      // 🔷 Header
                      Container(
                        width: double.infinity,
                        padding: const EdgeInsets.all(16),
                        decoration: BoxDecoration(
                          gradient: const LinearGradient(
                            colors: [Color(0xFF2563EB), Color(0xFF1E3A8A)],
                            begin: Alignment.topLeft,
                            end: Alignment.bottomRight,
                          ),
                          borderRadius: BorderRadius.circular(16),
                        ),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text("Today's Attendance",
                                style: GoogleFonts.poppins(
                                    color: Colors.white,
                                    fontSize: 18,
                                    fontWeight: FontWeight.w600)),
                            const SizedBox(height: 6),
                            Text(formattedDate,
                                style: GoogleFonts.inter(
                                    color: Colors.white70, fontSize: 14)),
                            const SizedBox(height: 4),
                            Text("$teacherName • $className",
                                style: GoogleFonts.inter(
                                    color: Colors.white70, fontSize: 13)),
                          ],
                        ),
                      ),
                      const SizedBox(height: 20),

                      // 🧒 Attendance list
                      Expanded(
                        child: ListView.builder(
                          itemCount: records.length,
                          itemBuilder: (context, index) {
                            final record = records[index];
                            final childName = record["childName"] ??
                                record["child"]?["fullName"] ??
                                "Unknown";

                            final status = record["status"] ?? "Present";
                            final color = status == "Present"
                                ? Colors.green
                                : status == "Late"
                                    ? Colors.orange
                                    : Colors.red;

                            return Card(
                              shape: RoundedRectangleBorder(
                                  borderRadius: BorderRadius.circular(16)),
                              margin:
                                  const EdgeInsets.symmetric(vertical: 8),
                              elevation: 2,
                              child: Padding(
                                padding: const EdgeInsets.all(14),
                                child: Row(
                                  mainAxisAlignment:
                                      MainAxisAlignment.spaceBetween,
                                  children: [
                                    Column(
                                      crossAxisAlignment:
                                          CrossAxisAlignment.start,
                                      children: [
                                        Text(
                                          childName,
                                          style: GoogleFonts.poppins(
                                              fontSize: 16,
                                              fontWeight: FontWeight.w600),
                                        ),
                                        const SizedBox(height: 4),
                                        Text("Status:",
                                            style: GoogleFonts.inter(
                                                color: Colors.grey[700])),
                                      ],
                                    ),
                                    Container(
                                      padding: const EdgeInsets.symmetric(
                                          horizontal: 8),
                                      decoration: BoxDecoration(
                                        color: color.withOpacity(0.1),
                                        borderRadius:
                                            BorderRadius.circular(20),
                                      ),
                                      child: DropdownButton<String>(
                                        value: status,
                                        underline: const SizedBox(),
                                        iconEnabledColor: AppTheme.primary,
                                        items: const [
                                          DropdownMenuItem(
                                              value: "Present",
                                              child: Text("Present")),
                                          DropdownMenuItem(
                                              value: "Late",
                                              child: Text("Late")),
                                          DropdownMenuItem(
                                              value: "Absent",
                                              child: Text("Absent")),
                                        ],
                                        onChanged: (val) {
                                          setState(() =>
                                              records[index]["status"] = val);
                                        },
                                      ),
                                    ),
                                  ],
                                ),
                              ),
                            );
                          },
                        ),
                      ),

                      // 💾 Save Button
                      SizedBox(
                        width: double.infinity,
                        child: FilledButton.icon(
                          onPressed: saveAttendance,
                          icon: const Icon(Icons.save_alt),
                          label: const Text("Save Attendance"),
                          style: FilledButton.styleFrom(
                            backgroundColor: AppTheme.primary,
                            padding: const EdgeInsets.symmetric(
                                vertical: 14, horizontal: 20),
                            shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(14)),
                          ),
                        ),
                      ),
                    ],
                  ),
                ),
    );
  }
}
