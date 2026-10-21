using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using PruebaTecnicaPkScs.Models;
using System.Net;

namespace PruebaTecnicaPkScs.Services
{
    public class EmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendPokemonEmailAsync(
            string recipientEmail,
            IReadOnlyCollection<PokemonApiItem> pokemon,
            string? nameFilter,
            string? speciesFilter,
            int page,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_settings.SmtpHost)
                || string.IsNullOrWhiteSpace(_settings.SenderEmail)
                || string.IsNullOrWhiteSpace(_settings.Username)
                || string.IsNullOrWhiteSpace(_settings.Password))
            {
                throw new InvalidOperationException(
                    "El servicio de correo no esta configurado. " +
                    "Revisa la configuración EmailSettings.");
            }

            if (pokemon.Count == 0)
            {
                throw new ArgumentException(
                    "No hay Pokémon para enviar.",
                    nameof(pokemon));
            }

            var message = new MimeMessage();

            message.From.Add(
                new MailboxAddress(
                    _settings.SenderName,
                    _settings.SenderEmail));

            message.To.Add(
                MailboxAddress.Parse(recipientEmail));

            message.Subject = pokemon.Count == 1
                ? $"Pokemon API: {pokemon.First().Name}"
                : $"Pokemon API: {pokemon.Count} Pokémon de la página {page}";

            var htmlItems = string.Join(
                Environment.NewLine,
                pokemon.Select(item =>
                {
                    string safeName = WebUtility.HtmlEncode(item.Name);
                    string safeImageUrl = WebUtility.HtmlEncode(item.ImageUrl);

                    return
                        $"<li><strong>{safeName}</strong> " +
                        $"(ID: {item.Id}) - " +
                        $"<a href=\"{safeImageUrl}\">Ver imagen</a></li>";
                }));

            string safeNameFilter = WebUtility.HtmlEncode(
                string.IsNullOrWhiteSpace(nameFilter)
                    ? "Todos"
                    : nameFilter);

            string safeSpeciesFilter = WebUtility.HtmlEncode(
                string.IsNullOrWhiteSpace(speciesFilter)
                    ? "Todas"
                    : speciesFilter);

            string textItems = string.Join(
                Environment.NewLine,
                pokemon.Select(item =>
                    $"- {item.Name} (ID: {item.Id})\n" +
                    $"  Imagen: {item.ImageUrl}"));

            var body = new BodyBuilder
            {
                HtmlBody = $"""
                <h1>Pokemon API</h1>
                <p>Listado de Pokémon.</p>
                <p><strong>Página:</strong> {page}</p>
                <ul>{htmlItems}</ul>
                """,

                TextBody = $"""
                Pokemon API
                Página: {page}

                {textItems}
                """
            };

            message.Body = body.ToMessageBody();

            using var client = new SmtpClient();

            await client.ConnectAsync(
                _settings.SmtpHost,
                _settings.SmtpPort,
                SecureSocketOptions.StartTls,
                cancellationToken);

            await client.AuthenticateAsync(
                _settings.Username,
                _settings.Password,
                cancellationToken);

            await client.SendAsync(message, cancellationToken);

            await client.DisconnectAsync(
                true,
                cancellationToken);
        }
    }
}
