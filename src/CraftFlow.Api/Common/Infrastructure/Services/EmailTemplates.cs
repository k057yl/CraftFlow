namespace CraftFlow.Api.Common.Infrastructure.Services;
public static class EmailTemplates
{
    public static (string Subject, string HtmlBody) GetOtpTemplate(string otpCode, string culture) => culture.ToLowerInvariant() switch
    {
        "uk-ua" or "uk" => (
            "CraftFlow: Одноразовий код входу",
            $@"
            <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f8f9fa;'>
                <div style='max-width: 500px; margin: 0 auto; background: #ffffff; padding: 25px; border-radius: 8px; border: 1px solid #e9ecef;'>
                    <h2 style='color: #212529; margin-top: 0;'>Вхід до CraftFlow</h2>
                    <p style='color: #495057;'>Ваш одноразовий код авторизації:</p>
                    <div style='background: #f1f3f5; font-size: 28px; font-weight: bold; letter-spacing: 6px; text-align: center; padding: 12px; margin: 20px 0; border-radius: 6px; color: #1c7ed6;'>
                        {otpCode}
                    </div>
                    <p style='font-size: 13px; color: #868e96;'>Код дійсний 5 хвилин. Якщо ви не запитували вхід, просто ігноруйте цей лист.</p>
                </div>
            </div>"
        ),

        "en-us" or "en" => (
            "CraftFlow: One-Time Login Code",
            $@"
            <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f8f9fa;'>
                <div style='max-width: 500px; margin: 0 auto; background: #ffffff; padding: 25px; border-radius: 8px; border: 1px solid #e9ecef;'>
                    <h2 style='color: #212529; margin-top: 0;'>Sign in to CraftFlow</h2>
                    <p style='color: #495057;'>Your one-time authorization code:</p>
                    <div style='background: #f1f3f5; font-size: 28px; font-weight: bold; letter-spacing: 6px; text-align: center; padding: 12px; margin: 20px 0; border-radius: 6px; color: #1c7ed6;'>
                        {otpCode}
                    </div>
                    <p style='font-size: 13px; color: #868e96;'>The code is valid for 5 minutes. If you did not request this, please ignore this email.</p>
                </div>
            </div>"
        ),

        _ => (
            "CraftFlow: Одноразовый код входа",
            $@"
            <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f8f9fa;'>
                <div style='max-width: 500px; margin: 0 auto; background: #ffffff; padding: 25px; border-radius: 8px; border: 1px solid #e9ecef;'>
                    <h2 style='color: #212529; margin-top: 0;'>Вход в CraftFlow</h2>
                    <p style='color: #495057;'>Ваш одноразовый код авторизации:</p>
                    <div style='background: #f1f3f5; font-size: 28px; font-weight: bold; letter-spacing: 6px; text-align: center; padding: 12px; margin: 20px 0; border-radius: 6px; color: #1c7ed6;'>
                        {otpCode}
                    </div>
                    <p style='font-size: 13px; color: #868e96;'>Код действителен 5 минут. Если вы не запрашивали вход, просто проигнорируйте это письмо.</p>
                </div>
            </div>"
        )
    };
}
