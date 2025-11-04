import 'package:flutter/material.dart';
import '../theme/app_theme.dart';

class TlinkyButton extends StatelessWidget {
  final String label;
  final VoidCallback onPressed;
  final bool primary;
  final bool danger;

  const TlinkyButton({
    super.key,
    required this.label,
    required this.onPressed,
    this.primary = false,
    this.danger = false,
  });

  @override
  Widget build(BuildContext context) {
    Color bg = danger
        ? AppTheme.danger
        : primary
            ? AppTheme.primary
            : Colors.grey[200]!;

    Color textColor = danger || primary ? Colors.white : Colors.black87;

    return ElevatedButton(
      onPressed: onPressed,
      style: ElevatedButton.styleFrom(
        backgroundColor: bg,
        foregroundColor: textColor,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      ),
      child: Text(label),
    );
  }
}
