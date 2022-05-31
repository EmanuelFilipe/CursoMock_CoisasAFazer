using Alura.CoisasAFazer.Core.Commands;
using Alura.CoisasAFazer.Core.Models;
using Alura.CoisasAFazer.Infrastructure;
using Alura.CoisasAFazer.Services.Handlers;
using Castle.Core.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace CoisasAFazer.Testes
{
    public class CadastraTarefaHandlerExecute
    {
        public DbContextOptions options;

        public CadastraTarefaHandlerExecute()
        {
            options = new DbContextOptionsBuilder<DbTarefasContext>()
                  .UseInMemoryDatabase("DbTarefasContext")
                  .Options;
        }

        [Fact]
        public void DadaTarefaComInfoValidasDeveIncluirNoBD()
        {
            //arrange
            var comando = new CadastraTarefa("Estudar xUnit", new Categoria("Estudo"), new DateTime(2019, 12, 31));

            var mock = new Mock<ILogger<CadastraTarefaHandler>>().Object;
            var contexto = new DbTarefasContext(options);
            var repo = new RepositorioTarefa(contexto);

            var handler = new CadastraTarefaHandler(repo, mock);
            
            //act
            handler.Execute(comando);

            //assert
            var tarefas = repo.ObtemTarefas(t => t.Titulo == "Estudar xUnit").FirstOrDefault();
            Assert.NotNull(tarefas);
        }

        [Fact]
        public void QuandoExceptionForLancadaResultadoIsSuccessDeveSerFalse()
        {
            //arrange
            var comando = new CadastraTarefa("Estudar xUnit", new Categoria("Estudo"), new DateTime(2019, 12, 31));

            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>().Object;
            var mock = new Mock<IRepositorioTarefas>();
            
            // mock irá fazer um setup para quando o método incluirTarefas for chamado,
            // para qualquer argumento de entrada do tipo Tarefa[] irá lançar uma exceção
            mock.Setup(r => r.IncluirTarefas(It.IsAny<Tarefa[]>()))
                .Throws(new Exception("Houve um erro na inclusão de tarefas"));

            //var contexto = new DbTarefasContext(options);
            var repo = mock.Object;

            var handler = new CadastraTarefaHandler(repo, mockLogger);

            //act
            handler.Execute(comando);

            //assert
            var tarefas = repo.ObtemTarefas(t => t.Titulo == "Estudar xUnit").FirstOrDefault();
            Assert.NotNull(tarefas);
        }

        [Fact]
        public void QuandoExceptionForLancadaDeveLogarAMensagemDaExcecao()
        {
            //arrange
            var comando = new CadastraTarefa("Estudar xUnit", new Categoria("Estudo"), new DateTime(2019, 12, 31));
            var mensagemErro = "Houve um erro na inclusão de tarefas";
            var excecaoEsperada = new Exception(mensagemErro);

            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>();
            var mock = new Mock<IRepositorioTarefas>();

            // mock irá fazer um setup para quando o método incluirTarefas for chamado,
            // para qualquer argumento de entrada do tipo Tarefa[] irá lançar uma exceção
            mock.Setup(r => r.IncluirTarefas(It.IsAny<Tarefa[]>()))
                .Throws(excecaoEsperada);

            //var contexto = new DbTarefasContext(options);
            var repo = mock.Object;

            var handler = new CadastraTarefaHandler(repo, mockLogger.Object);

            //act
            handler.Execute(comando);

            //assert
            mockLogger.Verify(l => l.Log(
                LogLevel.Error,      // nivel de log => logError
                It.IsAny<EventId>(), // identificador do evento
                It.IsAny<object>(),  // objeto que será logado
                excecaoEsperada,     // exceção que será logada
                It.IsAny<Func<object, Exception, string>>()), // função que converte objeto+excelçao em string
                Times.Once());
        }
    }
}
