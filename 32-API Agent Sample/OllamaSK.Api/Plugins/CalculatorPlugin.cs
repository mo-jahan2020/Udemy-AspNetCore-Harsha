using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace OllamaSK.Api.Plugins;

/// <summary>
/// Plugin ماشین‌حساب — یک Plugin واقعی که عملیات ریاضی را اجرا می‌کند.
/// این Plugin هم از طریق Function Calling خودکار Semantic Kernel قابل فراخوانی است
/// و هم مستقیماً از طریق endpoint «POST /api/tools/calculate».
/// ویژگی [Description] به مدل کمک می‌کند بفهمد هر تابع چه کاری انجام می‌دهد.
/// </summary>
public sealed class CalculatorPlugin
{
    [KernelFunction("Add")]
    [Description("Adds two numbers (جمع دو عدد)")]
    public double Add(
        [Description("First number")] double a,
        [Description("Second number")] double b) => a + b;

    [KernelFunction("Subtract")]
    [Description("Subtracts b from a (تفریق b از a)")]
    public double Subtract(
        [Description("First number")] double a,
        [Description("Second number")] double b) => a - b;

    [KernelFunction("Multiply")]
    [Description("Multiplies two numbers (ضرب دو عدد)")]
    public double Multiply(
        [Description("First number")] double a,
        [Description("Second number")] double b) => a * b;

    [KernelFunction("Divide")]
    [Description("Divides a by b (تقسیم a بر b)")]
    public double Divide(
        [Description("Dividend")] double a,
        [Description("Divisor (must not be zero)")] double b)
        => b == 0
            ? throw new ArgumentException("تقسیم بر صفر ممکن نیست.")
            : a / b;

    [KernelFunction("Power")]
    [Description("Raises a to the power of b (توان)")]
    public double Power(
        [Description("Base")] double a,
        [Description("Exponent")] double b) => Math.Pow(a, b);
}
