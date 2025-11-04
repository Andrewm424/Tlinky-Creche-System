import 'dart:convert';
import 'dart:io';
import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:image_picker/image_picker.dart';
import '../../theme/app_theme.dart';
import '../../widgets/tlinky_parent_drawer.dart';
import '../../services/api_service.dart';
import 'package:http/http.dart' as http;

class PaymentsScreen extends StatefulWidget {
  const PaymentsScreen({super.key});

  @override
  State<PaymentsScreen> createState() => _PaymentsScreenState();
}

class _PaymentsScreenState extends State<PaymentsScreen> {
  bool _loading = true;
  Map<String, dynamic>? _data;
  final ImagePicker _picker = ImagePicker();

  @override
  void initState() {
    super.initState();
    _fetchPayments();
  }

  Future<void> _fetchPayments() async {
    try {
      final data = await ApiService.getParentPayments(ParentSession.id ?? 0);
      setState(() {
        _data = data;
        _loading = false;
      });
    } catch (e) {
      setState(() => _loading = false);
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text("Error loading payments: $e")),
      );
    }
  }

  Future<void> _uploadProof(int paymentId) async {
    try {
      final XFile? picked =
          await _picker.pickImage(source: ImageSource.gallery);
      if (picked == null) return;

      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text("Uploading proof, please wait...")),
      );

      final uri = Uri.parse("${ApiService.baseUrl}/PaymentApi/UploadProof");
      final request = http.MultipartRequest('POST', uri)
        ..fields['paymentId'] = paymentId.toString()
        ..files.add(await http.MultipartFile.fromPath('file', picked.path));

      final streamed = await request.send();
      final response = await http.Response.fromStream(streamed);

      debugPrint('📡 Upload status: ${response.statusCode}');
      debugPrint('📡 Upload body: ${response.body}');

      if (response.statusCode == 200) {
        final json = jsonDecode(response.body);
        if (json['success'] == true) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text("✅ Proof uploaded successfully")),
          );
          _fetchPayments();
        } else {
          throw Exception(json['message'] ?? "Unknown error");
        }
      } else {
        throw Exception(
            "Upload failed (HTTP ${response.statusCode}): ${response.body}");
      }
    } catch (e) {
      debugPrint('❌ Upload error: $e');
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text("❌ Upload failed: $e")),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_loading) {
      return const Scaffold(
        backgroundColor: AppTheme.background,
        body: Center(child: CircularProgressIndicator()),
      );
    }

    final payments = List<Map<String, dynamic>>.from(_data?['payments'] ?? []);
    final balance = _data?['balance'] ?? 0;

    return Scaffold(
      drawer: const TlinkyParentDrawer(),
      backgroundColor: AppTheme.background,
      appBar: AppBar(
        backgroundColor: AppTheme.primary,
        foregroundColor: Colors.white,
        title: const Text("Payments"),
      ),
      body: Column(
        children: [
          Container(
            margin: const EdgeInsets.all(16),
            padding: const EdgeInsets.all(20),
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(16),
              boxShadow: const [BoxShadow(color: Colors.black12, blurRadius: 6)],
            ),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text("Outstanding Balance:",
                    style: GoogleFonts.poppins(
                        fontSize: 16, fontWeight: FontWeight.w500)),
                Text("R$balance",
                    style: GoogleFonts.poppins(
                        color: Colors.red,
                        fontWeight: FontWeight.bold,
                        fontSize: 18)),
              ],
            ),
          ),
          Expanded(
            child: ListView.builder(
              padding: const EdgeInsets.all(16),
              itemCount: payments.length,
              itemBuilder: (context, index) {
                final p = payments[index];
                final status =
                    (p['Status'] ?? p['status'] ?? 'Pending').toString();
                final color = status == 'Approved'
                    ? Colors.green
                    : status == 'Pending'
                        ? Colors.orange
                        : Colors.red;

                // Upload button visible unless status == Approved
                final canUpload = status != 'Approved';

                return Card(
                  shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(16)),
                  margin: const EdgeInsets.symmetric(vertical: 8),
                  child: Padding(
                    padding:
                        const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
                    child: Row(
                      children: [
                        const Icon(Icons.receipt_long, color: Colors.indigo),
                        const SizedBox(width: 12),
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                "${p['Month'] ?? p['month']} • R${p['Amount'] ?? p['amount']}",
                                style: GoogleFonts.poppins(
                                    fontSize: 16, fontWeight: FontWeight.w600),
                              ),
                              const SizedBox(height: 4),
                              Text("Status: $status",
                                  style: GoogleFonts.inter(
                                    fontSize: 13,
                                    color: color,
                                  )),
                              if (p['ProofUrl'] != null &&
                                  p['ProofUrl'].toString().isNotEmpty)
                                Padding(
                                  padding: const EdgeInsets.only(top: 8),
                                  child: ClipRRect(
                                    borderRadius: BorderRadius.circular(8),
                                    child: Image.network(
                                      p['ProofUrl'],
                                      height: 70,
                                      fit: BoxFit.cover,
                                    ),
                                  ),
                                ),
                            ],
                          ),
                        ),
                        if (canUpload)
                          FilledButton(
                            onPressed: () => _uploadProof(
                                p['PaymentId'] ?? p['paymentId'] ?? 0),
                            style: FilledButton.styleFrom(
                              backgroundColor: AppTheme.primary,
                              shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(10),
                              ),
                            ),
                            child: const Text("Upload"),
                          )
                        else
                          const Icon(Icons.check_circle,
                              color: Colors.green, size: 28),
                      ],
                    ),
                  ),
                );
              },
            ),
          ),
        ],
      ),
    );
  }
}
