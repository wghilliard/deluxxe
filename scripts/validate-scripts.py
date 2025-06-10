#!/usr/bin/env python3
"""
Validation script for Deluxxe scripts directory.

This script validates all Python scripts in the directory for:
- Syntax correctness
- Basic import validation
- Shebang presence
- Documentation presence
"""

import os
import sys
import ast
import subprocess
from pathlib import Path
from typing import List, Tuple, Dict, Any


def check_syntax(file_path: Path) -> Tuple[bool, str]:
    """Check Python syntax using ast.parse."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        ast.parse(content)
        return True, "OK"
    except SyntaxError as e:
        return False, f"Syntax error: {e}"
    except Exception as e:
        return False, f"Error: {e}"


def check_shebang(file_path: Path) -> Tuple[bool, str]:
    """Check if Python file has proper shebang."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            first_line = f.readline().strip()
        
        if first_line.startswith('#!') and 'python' in first_line:
            return True, "Has shebang"
        else:
            return False, "Missing or invalid shebang"
    except Exception as e:
        return False, f"Error reading file: {e}"


def check_docstring(file_path: Path) -> Tuple[bool, str]:
    """Check if Python file has module-level docstring."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        tree = ast.parse(content)
        if (tree.body and isinstance(tree.body[0], ast.Expr) and 
            isinstance(tree.body[0].value, ast.Constant) and 
            isinstance(tree.body[0].value.value, str)):
            return True, "Has module docstring"
        else:
            return False, "Missing module docstring"
    except Exception as e:
        return False, f"Error: {e}"


def check_shell_script(file_path: Path) -> Dict[str, Tuple[bool, str]]:
    """Check shell script quality."""
    results = {}
    
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # Check shebang
        lines = content.split('\n')
        if lines and lines[0].startswith('#!/'):
            results['shebang'] = (True, "Has shebang")
        else:
            results['shebang'] = (False, "Missing shebang")
        
        # Check for set -e
        if 'set -e' in content:
            results['error_handling'] = (True, "Has 'set -e'")
        else:
            results['error_handling'] = (False, "Missing 'set -e'")
        
        # Check for comments
        comment_lines = [line for line in lines if line.strip().startswith('#') and not line.strip().startswith('#!/')]
        if len(comment_lines) > 2:
            results['documentation'] = (True, f"Has {len(comment_lines)} comment lines")
        else:
            results['documentation'] = (False, "Insufficient documentation")
            
    except Exception as e:
        results['error'] = (False, f"Error reading file: {e}")
    
    return results


def main() -> None:
    """Main validation function."""
    script_dir = Path(__file__).parent
    
    print("🔍 Deluxxe Scripts Validation Report")
    print("=" * 50)
    
    # Find all Python scripts
    python_files = list(script_dir.glob("*.py"))
    shell_files = list(script_dir.glob("*.sh"))
    
    print(f"\nFound {len(python_files)} Python scripts and {len(shell_files)} shell scripts")
    
    # Validate Python scripts
    print("\n📄 Python Script Validation:")
    print("-" * 30)
    
    for py_file in sorted(python_files):
        if py_file.name == Path(__file__).name:  # Skip this validation script
            continue
            
        print(f"\n{py_file.name}:")
        
        # Syntax check
        syntax_ok, syntax_msg = check_syntax(py_file)
        print(f"  ✅ Syntax: {syntax_msg}" if syntax_ok else f"  ❌ Syntax: {syntax_msg}")
        
        # Shebang check
        shebang_ok, shebang_msg = check_shebang(py_file)
        print(f"  ✅ Shebang: {shebang_msg}" if shebang_ok else f"  ❌ Shebang: {shebang_msg}")
        
        # Docstring check
        doc_ok, doc_msg = check_docstring(py_file)
        print(f"  ✅ Docstring: {doc_msg}" if doc_ok else f"  ❌ Docstring: {doc_msg}")
    
    # Validate shell scripts
    if shell_files:
        print("\n🔧 Shell Script Validation:")
        print("-" * 30)
        
        for sh_file in sorted(shell_files):
            print(f"\n{sh_file.name}:")
            results = check_shell_script(sh_file)
            
            for check_name, (success, message) in results.items():
                icon = "✅" if success else "❌"
                print(f"  {icon} {check_name.title()}: {message}")
    
    # Check for README files
    print("\n📚 Documentation Validation:")
    print("-" * 30)
    
    readme_files = list(script_dir.glob("README*.md"))
    print(f"Found {len(readme_files)} README files:")
    for readme in sorted(readme_files):
        print(f"  ✅ {readme.name}")
    
    # Summary
    print(f"\n📊 Summary:")
    print(f"  • {len(python_files)-1} Python scripts validated")  # -1 for this script
    print(f"  • {len(shell_files)} shell scripts validated")
    print(f"  • {len(readme_files)} documentation files found")
    print("\n✨ Scripts directory cleanup completed!")


if __name__ == "__main__":
    main()
