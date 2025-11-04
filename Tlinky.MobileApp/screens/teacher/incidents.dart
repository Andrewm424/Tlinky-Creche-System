import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../theme/app_theme.dart';
import '../../widgets/tlinky_drawer.dart';
import '../../services/api_service.dart';
import 'package:http/http.dart' as http;
import 'dart:convert';

class IncidentsScreen extends StatefulWidget {
  const IncidentsScreen({super.key});

  @override
  State<IncidentsScreen> createState() => _IncidentsScreenState();
}

class _IncidentsScreenState extends State<IncidentsScreen> {
  final _formKey = GlobalKey<FormState>();
  List<dynamic> _children = [];
  final List<String> _incidentTypes = ['Minor Fall', 'Allergy', 'Illness', 'Other'];

  String? _selectedChild;
  String? _selectedType;
  final TextEditingController _descCtrl = TextEditingController();
  bool _loading = false;

  @override
  void initState() {
    super.initState();
    fetchChildren();
  }

  Future<void> fetchChildren() async {
    try {
      final classId = TeacherSession.teacher?['classId'] ?? 1;
      final url = Uri.parse('${ApiService.baseUrl}/ChildrenApi?classId=$classId');
      final response = await http.get(url);
      if (response.statusCode == 200) {
        final json = jsonDecode(response.body);
        setState(() {
          _children = json['data'] ?? [];
        });
      }
    } catch (e) {
      debugPrint('⚠️ Error loading children: $e');
    }
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate() ||
        _selectedChild == null ||
        _selectedType == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please complete all fields.')),
      );
      return;
    }

    setState(() => _loading = true);

    final teacherId = TeacherSession.teacher?['teacherId'];
    final classId = TeacherSession.teacher?['classId'];

    final incident = {
      "childId": int.tryParse(_selectedChild ?? ''),
      "teacherId": teacherId,
      "classId": classId,
      "type": _selectedType,
      "description": _descCtrl.text,
    };

    final uri = Uri.parse('${ApiService.baseUrl}/IncidentApi');
    print("📤 Sending incident payload: $incident");

    final res = await http.post(
      uri,
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode(incident),
    );

    setState(() => _loading = false);

    if (res.statusCode == 200) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Incident submitted successfully!')),
      );
      _descCtrl.clear();
      setState(() {
        _selectedChild = null;
        _selectedType = null;
      });
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Failed to submit (${res.statusCode})')),
      );
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
        title: const Text('Incident Report'),
        elevation: 0,
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Card(
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
          elevation: 2,
          child: Padding(
            padding: const EdgeInsets.all(20),
            child: Form(
              key: _formKey,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text("Record an Incident",
                      style: GoogleFonts.poppins(
                          fontSize: 18, fontWeight: FontWeight.w600)),
                  const SizedBox(height: 20),
                  Text("Select Child",
                      style: GoogleFonts.inter(fontWeight: FontWeight.w600)),
                  const SizedBox(height: 6),
                  DropdownButtonFormField<String>(
                    value: _selectedChild,
                    decoration: const InputDecoration(
                      border: OutlineInputBorder(),
                      hintText: "Choose child",
                    ),
                    items: _children
                        .map((c) => DropdownMenuItem(
                            value: c["childId"].toString(),
                            child: Text(c["fullName"])))
                        .toList(),
                    onChanged: (v) => setState(() => _selectedChild = v),
                    validator: (v) => v == null ? 'Please select a child' : null,
                  ),
                  const SizedBox(height: 16),
                  Text("Incident Type",
                      style: GoogleFonts.inter(fontWeight: FontWeight.w600)),
                  const SizedBox(height: 6),
                  DropdownButtonFormField<String>(
                    value: _selectedType,
                    decoration: const InputDecoration(
                      border: OutlineInputBorder(),
                      hintText: "Choose incident type",
                    ),
                    items: _incidentTypes
                        .map((t) =>
                            DropdownMenuItem(value: t, child: Text(t)))
                        .toList(),
                    onChanged: (v) => setState(() => _selectedType = v),
                    validator: (v) =>
                        v == null ? 'Please select incident type' : null,
                  ),
                  const SizedBox(height: 16),
                  Text("Description",
                      style: GoogleFonts.inter(fontWeight: FontWeight.w600)),
                  const SizedBox(height: 6),
                  TextFormField(
                    controller: _descCtrl,
                    maxLines: 4,
                    decoration: const InputDecoration(
                      border: OutlineInputBorder(),
                      hintText: "Describe what happened...",
                    ),
                    validator: (v) =>
                        (v == null || v.isEmpty) ? 'Please enter a description' : null,
                  ),
                  const SizedBox(height: 20),
                  SizedBox(
                    width: double.infinity,
                    child: FilledButton.icon(
                      onPressed: _loading ? null : _submit,
                      icon: const Icon(Icons.send_rounded),
                      label: _loading
                          ? const Text("Submitting...")
                          : const Text("Submit Report"),
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
          ),
        ),
      ),
    );
  }
}
